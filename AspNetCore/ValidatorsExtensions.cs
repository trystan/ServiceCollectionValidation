using ServiceCollectionValidation.AspNetCore.Rules;
namespace ServiceCollectionValidation.AspNetCore;

public static class ValidatorsExtensions
{
    public static Validator AspNetCore(this Validators validators)
    {
        return validators.Default
            .With<ShouldValidateControllers>();
    }
}
