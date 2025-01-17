﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dinah.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ObjectExtensionsTests
{
	public struct NonEnum : IConvertible
	{
		#region non-implemented IConvertible methods
		public TypeCode GetTypeCode() => throw new NotImplementedException();
		public bool ToBoolean(IFormatProvider provider) => throw new NotImplementedException();
		public byte ToByte(IFormatProvider provider) => throw new NotImplementedException();
		public char ToChar(IFormatProvider provider) => throw new NotImplementedException();
		public DateTime ToDateTime(IFormatProvider provider) => throw new NotImplementedException();
		public decimal ToDecimal(IFormatProvider provider) => throw new NotImplementedException();
		public double ToDouble(IFormatProvider provider) => throw new NotImplementedException();
		public short ToInt16(IFormatProvider provider) => throw new NotImplementedException();
		public int ToInt32(IFormatProvider provider) => throw new NotImplementedException();
		public long ToInt64(IFormatProvider provider) => throw new NotImplementedException();
		public sbyte ToSByte(IFormatProvider provider) => throw new NotImplementedException();
		public float ToSingle(IFormatProvider provider) => throw new NotImplementedException();
		public string ToString(IFormatProvider provider) => throw new NotImplementedException();
		public object ToType(Type conversionType, IFormatProvider provider) => throw new NotImplementedException();
		public ushort ToUInt16(IFormatProvider provider) => throw new NotImplementedException();
		public uint ToUInt32(IFormatProvider provider) => throw new NotImplementedException();
		public ulong ToUInt64(IFormatProvider provider) => throw new NotImplementedException();
		#endregion
	}

	public enum ByteEnum : byte
	{
		None,
		One,
		Two,
		[System.ComponentModel.Description("three val")]
		Three,
		Four
	}

	public enum IntEnum
	{
		None,
		One,
		Two,
		[System.ComponentModel.Description("three val")]
		Three,
		Four
	}

	[Flags]
	public enum FlagsEnum
	{
		[System.ComponentModel.Description("no val")]
		None = 0,
		One = 1,
		[System.ComponentModel.Description("two val")]
		Two = 2,
		[System.ComponentModel.Description("four val")]
		Four = 4,
		Eight = 8
	}

	[TestClass]
	public class GetDescription
	{
		[TestMethod]
		public void non_enum_is_null()
			=> new NonEnum().GetDescription().Should().BeNull();

		[TestMethod]
		public void non_described_byte_enum_is_null()
			=> ByteEnum.Four.GetDescription().Should().BeNull();

		[TestMethod]
		public void described_byte_enum_is_foo()
			=> ByteEnum.Three.GetDescription().Should().Be("three val");

		[TestMethod]
		public void non_described_int_enum_is_null()
			=> IntEnum.Four.GetDescription().Should().BeNull();

		[TestMethod]
		public void described_int_enum_is_foo()
			=> IntEnum.Three.GetDescription().Should().Be("three val");

		[TestMethod]
		public void non_described_flag_enum_is_null()
			=> FlagsEnum.Eight.GetDescription().Should().BeNull();

		[TestMethod]
		public void described_flag_enum_is_foo()
			=> FlagsEnum.Four.GetDescription().Should().Be("four val");

		[TestMethod]
		public void mult_flag_enums_with_null()
			=> (FlagsEnum.Eight | FlagsEnum.Four).GetDescription().Should().Be("four val | [null]");

		[TestMethod]
		public void mult_flag_enums_descriptions()
			=> (FlagsEnum.Two | FlagsEnum.Four).GetDescription().Should().Be("two val | four val");

		[TestMethod]
		public void none_flag_enums_description()
			=> FlagsEnum.None.GetDescription().Should().Be("no val");
	}
}
