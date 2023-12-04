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
		Dictionary<string,object> varibles;
		public void Run(){
			try{
			varibles = new Dictionary<string,object>();
			varibles.Add("Result",0);
			Token[] program = Tokenize(Parse());
			if(err){
				Console.WriteLine($"Following error ocurred during parsing: {errMsg}");
				return;
			}
			for(int i = 0; i < program.Length; i++){
				Token token = program[i];
				if(token.Type == TokenType.Command){
					switch((Command)Enum.Parse(typeof(Command),InsertVarible(token))){
						case Command.Print:
							Console.Write(InsertVarible(program[++i]));
							break;
						case Command.Line:
							Console.WriteLine(InsertVarible(program[++i]));
							break;
						case Command.Var:
							string name = "";
							object type = null;
							object value = null;
							if(ValidateVarName(InsertVarible(program[++i]))){
								name = InsertVarible(program[i]);
								if(program[++i].Type == TokenType.Keyword && InsertVarible(program[i]) == "Type"){
									if(!Enum.TryParse(typeof(VarType),InsertVarible(program[++i]),out type)){
										Console.WriteLine(
											$"\nUnexpexted token \"{InsertVarible(program[i])}\" "+
											$"of type{program[i].Type}, expected token of type VaribleType"
										);
										return;
									}
									if(program[++i].Type == TokenType.Keyword && InsertVarible(program[i]) == "Value"){
										switch((VarType)type){
											case VarType.Integer:
												if(program[++i].Type == TokenType.Integer){
													value = program[i].Value;
												} else {
													Console.WriteLine(
														$"\nUnexpected token {InsertVarible(program[i])} of type {program[i].Type}"+
														$", expected token of type Integer"
													);
													return;
												}
												break;
											case VarType.Decimal:
												if(program[++i].Type == TokenType.Decimal || program[i].Type == TokenType.Integer){
													value = program[i].Value;
												} else {
													Console.WriteLine(
														$"\nUnexpected token {InsertVarible(program[i])} of type {program[i].Type}"+
                                                                                                        	$", expected token of type Decimal or Integer"
                                                                                                        );
                                                                                                        return;
                                                                                                }
												break;
											case VarType.String:
												value = InsertVarible(program[++i]);
												break;
										}
									}
								} else {
									Console.WriteLine(
										$"\nUnexpected token {InsertVarible(program[i])} of type {program[i].Type}"+
										$",expected token Type of type KeyWord"
									);
									return;
								}
							} else {
								Console.WriteLine(
									$"\n{InsertVarible(program[i])} is not valid varible name"
								);
								return;
							}
							if(!varibles.ContainsKey(name)){
								varibles.Add(name, value);
							} else {
								Console.WriteLine(
									$"\nVarible with name {name} already exist"
								);
							}
							break;
					}
				}
				else{
					Console.WriteLine($"\nUnexpected token {InsertVarible(token)} of type {token.Type}, expected token of type Command");
					return;
				}
			}
			} catch (ArgumentException e){
				Console.WriteLine($"\nFollowing exception occured during runtime: \n{e.Message}");
				return;
			}
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
			Var,
                }
		private enum Keyword{
			Type,
			Value,
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
			src = src.Replace("\r","");
			for(int i = 0; i < src.Length; i++){
				char sym = src[i];
				if(!escapeChar){
					switch (sym){	
						case '\n':
							line++;
							result.Add(str); str="";
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
						case '$':
							str += '\r';
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
						case '$':
							str+='$';
							break;
						case '"':
							str+='"';
							break;
						default:
							err = true;
							errMsg = $"Unexpected escape sequence \\{sym} at line {line}";
							break;
					}
					if(err) return null;
					escapeChar = false;
				}
			}
			if(insideQuotes){
				err = true;
				errMsg = $"Unexpected \"End Of File\", expected \"";
				return null;
			}
			result.Add(str);
			return result.ToArray();
		}
		private Token[] Tokenize(string[] src_p){
			List<Token> result = new List<Token>();
			for(int i = 0; i < src_p.Length; i++){
				switch(src_p[i]){
					case "":
						break;
					case "Var": goto case "Print";
					case "Print": goto case "Line";
					case "Line":
						result.Add(new Token(TokenType.Command, src_p[i]));
						break;
					case "String": goto case "Integer";
				 	case "Integer": goto case "Decimal";
					case "Decimal":
						result.Add(new Token(TokenType.VarType, src_p[i]));
						break;
					case "Type": goto case "Value";
					case "Value":
						result.Add(new Token(TokenType.Keyword, src_p[i]));
						break;
					default:
						if(long.TryParse(src_p[i],out long valL)) result.Add(new Token(TokenType.Integer, valL));
						else if (double.TryParse(src_p[i],NumberStyles.Number,new CultureInfo("En-Us"),out double valD)) result.Add(new Token(TokenType.Decimal, valD));
						else result.Add(new Token(TokenType.String, src_p[i]));
						break;
				}
			}
			return result.ToArray();
		}
		private bool ValidateVarName(string name){
			if(int.TryParse(name[0].ToString(),out int _)) return false;
			foreach(char letter in name){
				if(!char.IsLower(letter)&&!char.IsUpper(letter)&&letter!='_') return false;
			}
			return true;
		}
		private string InsertVarible(Token token){
			List<string> words = new List<string>(token.Value.ToString().Split(' '));
			for(int i = 0; i < words.Count; i++){
				string word = words[i];
				if(word[0] == '\r'){
					string name = word.Substring(1);
					if(ValidateVarName(name)){
						if(varibles.ContainsKey(name)){
							words.Remove('\r'+name);
							words.Insert(i, varibles[name].ToString());
						} else {
							errMsg = $"Varible \"{name}\" doesn't exist";
							throw new ArgumentException(errMsg);
						}
					} else {
						errMsg = $"\"{name}\" is not valid varible name";
						throw new ArgumentException(errMsg);
					}
				}
			}
			string result = "";
			foreach(string word in words){
				result += word+" ";
			}
			return result.Remove(result.Length-1);
		}
		public StealLang(string src){
			this.src = src;
			line = 0;
		}
	}
}
