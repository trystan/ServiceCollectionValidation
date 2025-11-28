using AspNetCore.Rules;
using ServiceCollectionValidation;
namespace AspNetCore;

public static class ValidatorsExtensions
{
    public static Validator AspNetCore(this Validators validators)
    {
        return validators.Default
            .With<ShouldValidateControllers>();
    }
}
