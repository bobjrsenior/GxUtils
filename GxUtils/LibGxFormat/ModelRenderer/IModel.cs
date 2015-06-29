
namespace LibGxFormat.ModelRenderer
{
    /// <summary>Interface to be implemented by each model instance.</summary>
    public interface IModel
    {
        /// <summary>Render this model using the given model renderer.</summary>
        /// <param name="renderer">The instance of the renderer to use to render this model.</param>
        void Render(IRenderer renderer);
    }
}
