
using System;
using UnSwallowExceptions.Fody;

namespace AssemblyToProcess
{
    public class OnException
    {
        public void Swallowed_exception()
        {
            try
            {
                throw new Exception("test");
            }
            catch (Exception)
            {
                
            }
        }

        [UnSwallowExceptions]
        public void Swallowed_exception_to_be_unswallowed()
        {
            try
            {
                throw new Exception("test");
            }
            catch (Exception)
            {

            }
        }

        public void Expected_unswallowed_exception()
        {
            throw new Exception();
        }
    }
}
