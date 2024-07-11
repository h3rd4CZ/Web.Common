namespace RhDev.Common.Workflow.Builder
{
    public abstract class WorkflowPartBuilderBase<TPart, TProduct>
        : IStateManagementBuilder<TProduct>
        where TPart : WorkflowPartBuilderBase<TPart, TProduct>, new()
        where TProduct : IWorkflowPartBuilder, new()
    {
        protected TProduct product;
        protected void Init()
        {
            product = new TProduct();
        }
        public static TPart New()
        {
            var part = new TPart();
            part.Init();

            return part;
        }

        public TProduct Build() => product;
    }
}
