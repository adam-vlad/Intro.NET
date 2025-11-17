using FluentValidation;
using Orders.Application.Abstractions;
using Orders.Application.Context;
using Orders.Application.Requests;
using Orders.Domain.Enums;
using System.Text.RegularExpressions;

namespace Orders.Application.Validation;

public class CreateOrderProfileValidator : AbstractValidator<CreateOrderProfileRequest>
{
    private readonly ApplicationContext _context;
    private readonly IOrderRepository _repository;
    private readonly ILogger<CreateOrderProfileValidator> _logger;

    private readonly string[] _inappropriateWords = { "badword1", "badword2", "offensive" };
    private readonly string[] _technicalKeywords = { "programming", "algorithm", "software", "database", "technical", "engineering", "computer", "code" };
    private readonly string[] _childrenInappropriateWords = { "violence", "adult", "scary", "horror" };

    public CreateOrderProfileValidator(
        ApplicationContext context,
        IOrderRepository repository,
        ILogger<CreateOrderProfileValidator> logger)
    {
        _context = context;
        _repository = repository;
        _logger = logger;

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("title is required")
            .MinimumLength(1).WithMessage("title must be at least 1 character")
            .MaximumLength(200).WithMessage("title must not exceed 200 characters")
            .Must(BeValidTitle).WithMessage("title contains inappropriate content")
            .MustAsync(BeUniqueTitle).WithMessage("title already exists for this author");

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("author is required")
            .MinimumLength(2).WithMessage("author must be at least 2 characters")
            .MaximumLength(100).WithMessage("author must not exceed 100 characters")
            .Must(BeValidAuthorName).WithMessage("author name contains invalid characters");

        RuleFor(x => x.ISBN)
            .NotEmpty().WithMessage("isbn is required")
            .Must(BeValidISBN).WithMessage("isbn format is invalid")
            .MustAsync(BeUniqueISBN).WithMessage("isbn already exists");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("category must be valid");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("price must be greater than 0")
            .LessThan(10000).WithMessage("price must be less than 10000");

        RuleFor(x => x.PublishedDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("published date cannot be in the future")
            .GreaterThanOrEqualTo(new DateTime(1400, 1, 1)).WithMessage("published date cannot be before year 1400");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("stock quantity cannot be negative")
            .LessThanOrEqualTo(100000).WithMessage("stock quantity cannot exceed 100000");

        RuleFor(x => x.CoverImageUrl)
            .Must(BeValidImageUrl).When(x => !string.IsNullOrWhiteSpace(x.CoverImageUrl))
            .WithMessage("cover image url must be valid");

        RuleFor(x => x)
            .MustAsync(PassBusinessRules).WithMessage("order does not pass business rules");

        When(x => x.Category == OrderCategory.Technical, () =>
        {
            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(20).WithMessage("technical orders must have price of at least 20");

            RuleFor(x => x.Title)
                .Must(ContainTechnicalKeywords).WithMessage("technical orders must contain technical keywords");

            RuleFor(x => x.PublishedDate)
                .GreaterThanOrEqualTo(DateTime.UtcNow.AddYears(-5)).WithMessage("technical orders must be published within last 5 years");
        });

        When(x => x.Category == OrderCategory.Children, () =>
        {
            RuleFor(x => x.Price)
                .LessThanOrEqualTo(50).WithMessage("children orders must have price of at most 50");

            RuleFor(x => x.Title)
                .Must(BeAppropriateForChildren).WithMessage("title is not appropriate for children");
        });

        When(x => x.Category == OrderCategory.Fiction, () =>
        {
            RuleFor(x => x.Author)
                .MinimumLength(5).WithMessage("fiction orders require full author name (minimum 5 characters)");
        });

        RuleFor(x => x)
            .Must(x => x.Price <= 100 || x.StockQuantity <= 20)
            .WithMessage("expensive orders (>100) must have limited stock (max 20 units)");
    }

    private bool BeValidTitle(string title)
    {
        foreach (var word in _inappropriateWords)
        {
            if (title.Contains(word, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }
        return true;
    }

    private async Task<bool> BeUniqueTitle(CreateOrderProfileRequest request, string title, CancellationToken cancellationToken)
    {
        _logger.LogInformation("validating title uniqueness for title: {Title}, author: {Author}", title, request.Author);
        var orders = await _repository.GetAllAsync();
        var exists = orders.Any(o => o.Title.Equals(title, StringComparison.OrdinalIgnoreCase) && o.Author.Equals(request.Author, StringComparison.OrdinalIgnoreCase));
        _logger.LogInformation("title uniqueness check result: {IsUnique}", !exists);
        return !exists;
    }

    private bool BeValidAuthorName(string author)
    {
        var pattern = @"^[a-zA-Z\s\-'.]+$";
        return Regex.IsMatch(author, pattern);
    }

    private bool BeValidISBN(string isbn)
    {
        var cleaned = isbn.Replace("-", "").Replace(" ", "");
        return (cleaned.Length == 10 || cleaned.Length == 13) && cleaned.All(char.IsDigit);
    }

    private async Task<bool> BeUniqueISBN(string isbn, CancellationToken cancellationToken)
    {
        _logger.LogInformation("validating isbn uniqueness for isbn: {ISBN}", isbn);
        var orders = await _repository.GetAllAsync();
        var exists = orders.Any(o => o.ISBN.Equals(isbn, StringComparison.OrdinalIgnoreCase));
        _logger.LogInformation("isbn uniqueness check result: {IsUnique}", !exists);
        return !exists;
    }

    private bool BeValidImageUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return false;

        var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        return validExtensions.Any(ext => uri.AbsolutePath.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<bool> PassBusinessRules(CreateOrderProfileRequest request, CancellationToken cancellationToken)
    {
        var todayCount = _context.GetTodayOrderCount();
        if (todayCount >= 500)
        {
            _logger.LogWarning("daily order limit reached: {Count}", todayCount);
            return false;
        }

        if (request.Category == OrderCategory.Technical && request.Price < 20.00m)
        {
            _logger.LogWarning("technical order price too low: {Price}", request.Price);
            return false;
        }

        if (request.Category == OrderCategory.Children)
        {
            foreach (var word in _childrenInappropriateWords)
            {
                if (request.Title.Contains(word, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("children order contains inappropriate word: {Word}", word);
                    return false;
                }
            }
        }

        if (request.Price > 500 && request.StockQuantity > 10)
        {
            _logger.LogWarning("high-value order exceeds stock limit: price {Price}, stock {Stock}", request.Price, request.StockQuantity);
            return false;
        }

        return true;
    }

    private bool ContainTechnicalKeywords(string title)
    {
        return _technicalKeywords.Any(keyword => title.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private bool BeAppropriateForChildren(string title)
    {
        foreach (var word in _childrenInappropriateWords)
        {
            if (title.Contains(word, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }
        return true;
    }
}