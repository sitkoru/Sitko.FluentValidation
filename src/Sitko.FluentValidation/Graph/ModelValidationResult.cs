namespace Sitko.FluentValidation.Graph;

#if NET6_0_OR_GREATER
using System.Globalization;
#endif
using System.Text;
using global::FluentValidation.Results;

public class ModelValidationResult : IEquatable<ModelValidationResult>
{
    public ModelValidationResult(object model) => Model = model;

    public ModelValidationResult(object model, IEnumerable<ValidationFailure> errors)
    {
        Model = model;
        Errors.AddRange(errors);
    }

    public object Model { get; }
    public bool IsValid => !Errors.Any();
    public List<ValidationFailure> Errors { get; } = new();

    public bool Equals(ModelValidationResult? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Model.Equals(other.Model);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((ModelValidationResult)obj);
    }

    public override int GetHashCode() => Model.GetHashCode();

    public override string ToString()
    {
        var result = new StringBuilder($"Model {Model}");
        if (IsValid)
        {
            result.Append(" is valid");
        }
        else
        {
            foreach (var validationFailure in Errors)
            {
                result.Append(
#if NET6_0_OR_GREATER
                    CultureInfo.InvariantCulture,
#endif
                    $"\n\t{validationFailure.PropertyName}: {validationFailure.ErrorMessage}");
            }
        }

        return result.ToString();
    }
}
