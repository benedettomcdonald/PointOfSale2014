using System;
using System.Collections;
using HsSharedObjects.Client;

namespace HsExecuteModule
{
    /// <summary>
    /// Finalize interface
    /// 
    /// Finalize will serve as a clean-up subroutine for various Execute implementations.
    /// </summary>
    public interface Finalize
    {
        void PerformFinalize();
        String GetXmlString();
        ClientDetails ClientDetails { get; set; }
    }
}
