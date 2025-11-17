using FluentValidation;
using AIContentRemover.DTOs;

namespace AIContentRemover.Validators;

/// <summary>
/// Validateur pour TagTweetRequest
/// </summary>
public class TagTweetRequestValidator : AbstractValidator<TagTweetRequest>
{
    public TagTweetRequestValidator()
    {
        RuleFor(x => x.TweetId)
            .NotEmpty().WithMessage("TweetId est requis")
            .MaximumLength(20).WithMessage("TweetId ne peut pas dépasser 20 caractères")
            .Matches(@"^\d+$").WithMessage("TweetId doit contenir uniquement des chiffres");

        RuleFor(x => x.UserIdentifier)
            .NotEmpty().WithMessage("UserIdentifier est requis")
            .MaximumLength(128).WithMessage("UserIdentifier ne peut pas dépasser 128 caractères");
    }
}

/// <summary>
/// Validateur pour BatchCheckRequest
/// </summary>
public class BatchCheckRequestValidator : AbstractValidator<BatchCheckRequest>
{
    public BatchCheckRequestValidator()
    {
        RuleFor(x => x.TweetIds)
            .NotNull().WithMessage("TweetIds ne peut pas être null")
            .Must(x => x.Count > 0).WithMessage("TweetIds doit contenir au moins un élément")
            .Must(x => x.Count <= 100).WithMessage("Maximum 100 tweets par requête batch");

        RuleForEach(x => x.TweetIds)
            .NotEmpty().WithMessage("Les TweetIds ne peuvent pas être vides")
            .MaximumLength(20).WithMessage("TweetId ne peut pas dépasser 20 caractères")
            .Matches(@"^\d+$").WithMessage("TweetId doit contenir uniquement des chiffres");
    }
}

