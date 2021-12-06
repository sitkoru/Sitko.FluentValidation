namespace Sitko.FluentValidation.Graph;

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
}
