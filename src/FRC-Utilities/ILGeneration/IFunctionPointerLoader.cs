using System;

namespace FRC.ILGeneration {
  public interface IFunctionPointerLoader {
    IntPtr GetProcAddress(string name);
  }
}
