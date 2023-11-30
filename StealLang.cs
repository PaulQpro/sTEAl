using System;
using System.Collections.Generic;
using System.Globalization;
namespace Steal{
	public class StealLang{
		int line;
		public void Run(){
			
		}
		private enum TokenType{
			Command,
			VarType,
			ID,
			Decimal,
			Integer,
			String,
			Logic,
		}
		private class Token{
			public TokenType Type {get;set;}
			public object Value {get;set;}
		}
	}
}
