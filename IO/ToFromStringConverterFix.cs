using System;
using SpikysLib.IO;

namespace SPIC.IO;

public sealed class ToFromStringConverterFix(Type type) : ToFromStringConverter(type) {}