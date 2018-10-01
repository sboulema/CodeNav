namespace CodeNav.Tests.Files
{
    class TestMethodsWithComments
    {
        /// <summary>
        /// Super important summary
        /// </summary>
        public void MethodWithComment()
        {

        }

        public void MethodWithoutComment()
        {

        }

        /// <summary>
        /// Multiple comment - summary
        /// </summary>
        /// <remarks>
        /// Multiple comment - remarks
        /// </remarks>
        public void MethodWithMultipleComment()
        {

        }

        /// <remarks>
        /// Multiple comment - remarks
        /// </remarks>
        /// <summary>
        /// Multiple comment - summary
        /// </summary>
        public void MethodWithReorderedComment()
        {

        }
    }
}
