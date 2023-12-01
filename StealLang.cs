//Written using VIM
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
namespace Steal{
	public class StealLang{
		int line;
		string src;
		bool err;
		string errMsg;
		public void Run(){
			Console.WriteLine(JsonSerializer.Serialize(Tokenize(Parse())));	
		}
		private enum TokenType{
			Command,
			Keyword,
			VarType,
			Decimal,
			Integer,
			String,
			Logic,
		}
		private enum VarType{
			String,
			Integer,
			Decimal,
		}
		private enum Command{
			Print,
                        Line,
			Type,
                }
		private class Token{
			public TokenType Type {get;set;}
			public object Value {get;set;}
			public Token(TokenType type, object val){
				Type = type;
				Value = val;
			}
		}
		private string[] Parse(){
			line = 0;
			List<string> result = new List<string>();
			string str = "";
			bool insideQuotes = false;
			bool escapeChar = false;
			for(int i = 0; i < src.Length; i++){
				char sym = src[i];
				if(!escapeChar){
					switch (sym){	
						case '\n':
							line++;
							break;
						case '\\':
							escapeChar = true;
							break;
						case '"': 
							insideQuotes = !insideQuotes;
							break;
						case ' ':
							if(insideQuotes) str+=sym;
							else { result.Add(str); str=""; }
							break;
						default:
							str+=sym;
							break;
					}
				}
				else{
					switch (sym){
						case 'n':
							str+='\n';
							break;
						case '0':
							str+='\0';
							break;
						case '\\':
							str+='\\';
							break;
						case '@':
							str+='@';
							break;
						case '"':
							str+='"';
							break;
						default:
							err = true;
							errMsg = $"Unexpected escape sequence \\{sym} at line {line}";
							break;
					}
					escapeChar = false;
				}
			}
			result.Add(str);
			return result.ToArray();
		}
		private Token[] Tokenize(string[] src_p){
			List<Token> result = new List<Token>();
			for(int i = 0; i < src_p.Length; i++){
				switch(src_p[i]){
					case "Var": goto case "Print";
					case "Print": goto case "Line";
					case "Line":
						result.Add(new Token(TokenType.Command, src_p[i]));
						break;
					case "String": goto case "Integer";
				 	case "Integer": goto case "Decimal";
					case "Decimal":
						result.Add(new Token(TokenType.VarType, (VarType) Enum.Parse(typeof(VarType), src_p[i], true)));
						break;
					case "Type": goto case "Value";
					case "Value":
						result.Add(new Token(TokenType.Keyword, src_p[i]));
						break;
					default:
						if(long.TryParse(src_p[i], out long val_long)) result.Add(new Token(TokenType.Integer, val_long));
						else if (double.TryParse(src_p[i], out double val_double)) result.Add(new Token(TokenType.Decimal, val_double));
						else result.Add(new Token(TokenType.String, src_p[i]));
						break;
				}
			}
			return result.ToArray();
		}
		public StealLang(string src){
			this.src = src;
			line = 0;
		}
	}
}
