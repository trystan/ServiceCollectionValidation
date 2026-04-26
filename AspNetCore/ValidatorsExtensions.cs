using ServiceCollectionValidation.AspNetCore.Rules;

namespace ServiceCollectionValidation.AspNetCore;

public static class ValidatorsExtensions
{
    public static Validator AspNetCore(this Validators validators, TypeFilterOptions? options = null)
    {
        return validators.Default
            .WithBeforeValidation(new ShouldValidateControllers(options));
    }
}
