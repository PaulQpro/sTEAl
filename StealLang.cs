//Partialy written using VIM
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Steal{
	public class StealLang{
		int line;
		string src;
		bool err;
		string errMsg;
		Dictionary<string,object> varibles;
		public void Run(){//Main execution proccess
			try{
                varibles = new Dictionary<string, object>
                {
                    { "Result", 0L }
                };
                Token[] program = Tokenize(Parse());
			if(err){
				Console.WriteLine($"Following error ocurred during parsing: {errMsg}");
				return;
			}
			for(int i = 0; i < program.Length; i++){
				Token token = program[i];
				if(token.Type == TokenType.Command){
					switch((Command)Enum.Parse(typeof(Command),InsertVariable(token))){
						case Command.Print:
							Console.Write(InsertVariable(program[++i]));
							break;
						case Command.Line:
							Console.WriteLine(InsertVariable(program[++i]));
							break;
						case Command.Var:
							string name = "";
							VarType type;
							object value = null;
							if(ValidateVarName(InsertVariable(program[++i]))){
								name = InsertVariable(program[i]);
								if(program[++i].Type == TokenType.Keyword && InsertVariable(program[i]) == "Type"){
									if(!Enum.TryParse(InsertVariable(program[++i]),out type)){
										Console.WriteLine(
											$"\nUnexpexted token \"{InsertVariable(program[i])}\" "+
											$"of type{program[i].Type}, expected token of type VaribleType"
										);
										return;
									}
									if(program[++i].Type == TokenType.Keyword && InsertVariable(program[i]) == "Value"){
										switch(type){
											case VarType.Integer:
												if(program[++i].Type == TokenType.Integer){
													value = program[i].Value;
												} else {
													Console.WriteLine(
														$"\nUnexpected token {InsertVariable(program[i])} of type {program[i].Type}"+
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
														$"\nUnexpected token {InsertVariable(program[i])} of type {program[i].Type}"+
                                                        $", expected token of type Decimal or Integer"
                                                    );
                                                    return;
												}
												break;
											case VarType.String:
												value = InsertVariable(program[++i]);
												break;
											case VarType.Logic:
                                                if(program[++i].Type == TokenType.Logic)
                                                {
													value = program[i].Value;
                                                }
                                                else
                                                {
													Console.WriteLine(
														$"\nUnexpected token {InsertVariable(program[i])} of type {program[i].Type}" +
														$", expected token of type Logic"
													);
													return;
												}
												break;
										}
									}
								} else {
									Console.WriteLine(
										$"\nUnexpected token {InsertVariable(program[i])} of type {program[i].Type}"+
										$",expected token Type of type KeyWord"
									);
									return;
								}
							} else {
								Console.WriteLine(
									$"\n{InsertVariable(program[i])} is not valid varible name"
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
						case Command.PgmDump:
							Console.WriteLine("Program Dump");
							foreach(Token _token in program)
							{
								Console.WriteLine(JsonSerializer.Serialize(_token as object, new JsonSerializerOptions()
								{
									Converters =
									{
										new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
									}
								}));
							}
							break;
						case Command.VarDump:
							Console.WriteLine("Variables Dump");
							foreach(string _var in varibles.Keys)
                            {
								string _type = "";
                                switch (varibles[_var].GetType().FullName)
                                {
									case "System.Int64":
										_type = "Integer";
										break;
									case "System.Decimal":
										_type = "Decimal";
										break;
									case "System.String":
										_type = "String";
										break;
                                }
								Console.WriteLine($"{{\"Name\":{_var},\"Type\":{ _type },\"Value\":{varibles[_var]}}}");
                            }
							break;
                        case Command.FullDump:
                            Console.WriteLine("Full Dump (Program then Variables");
                            foreach (Token _token in program)
                            {
                                Console.WriteLine(JsonSerializer.Serialize(_token as object, new JsonSerializerOptions()
                                {
                                    Converters =
                            {
                                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                            }
                                }));
                            }
                            foreach (string _var in varibles.Keys)
                            {
                                string _type = "";
                                switch (varibles[_var].GetType().FullName)
                                {
                                    case "System.Int64":
                                        _type = "Integer";
                                        break;
                                    case "System.Decimal":
                                        _type = "Decimal";
                                        break;
                                    case "System.String":
                                        _type = "String";
                                        break;
									case "System.Boolean":
										_type = "Logic";
										break;
                                }
                                Console.WriteLine($"{{\"Name\":{_var},\"Type\":{ _type },\"Value\":{varibles[_var]}}}");
                            }
                            break;
                        }
				}
				else{
					Console.WriteLine($"\nUnexpected token {InsertVariable(token)} of type {token.Type}, expected token of type Command");
					return;
				}
			}
			} catch (ArgumentException e){
				Console.WriteLine($"\nFollowing exception occured during runtime: \n{e.Message}");
				return;
			}
		}
		private enum TokenType{
			Command, //Executable operation
			Keyword, //Hardcoded indetefier
			VarType, //Type of variable/constant
			Decimal, //See VarType.Decimal
			Integer, //See VarType.Integer
			String, //See VarType.String
			Logic, //See VarType.Logic
		}
		private enum VarType{
			String, //string (char[]) (System.String)
			Integer, //long (int64) (System.Int64)
			Decimal, //decimal (float128) (System.Decimal)
			Logic, //bool (System.Boolean)
		}
		private enum Command{
			Print, //Print
            Line, //Print + new line
			Var, //Define variable
			PgmDump, //Print tokens of program
			VarDump, //Print varables
			FullDump, //Two above
			Comp, //Compute (Math)
			Eval, //Evaluate (Logic)
        }
		private enum Keyword{
			Type, //Type of variable/constant
			Value, //Value of varable/constant
		}
		private class Token{ //Basic unit of program
			public TokenType Type {get;set;}
			public object Value {get;set;}
			public Token(TokenType type, object val){
				Type = type;
				Value = val;
			}
		}
		private string[] Parse(){ //Parases source code into array of 'words' and 'sentences (multiple words)', also resolves escape-sequenses
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
		private Token[] Tokenize(string[] src_p){//Turn array of 'words' and 'sentences' into array of tokens to be executed
			List<Token> result = new List<Token>();
			for(int i = 0; i < src_p.Length; i++){
				switch(src_p[i]){
					case "":
						break;
					case "FullDump": goto case "PgmDump";
					case "VarDump": goto case "PgmDump";
					case "PgmDump": goto case "Var";
					case "Var": goto case "Print";
					case "Print": goto case "Line";
					case "Line":
						result.Add(new Token(TokenType.Command, src_p[i]));
						break;
					case "Logic": goto case "String";
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
						if (long.TryParse(src_p[i], out long valL)) result.Add(new Token(TokenType.Integer, valL));
						else if (decimal.TryParse(src_p[i], NumberStyles.Number, new CultureInfo("En-Us"), out decimal valD)) result.Add(new Token(TokenType.Decimal, valD));
						else if (src_p[i] == "True") result.Add(new Token(TokenType.Logic, src_p[i])); else if (src_p[i] == "False") result.Add(new Token(TokenType.Logic, src_p[i]));
						else result.Add(new Token(TokenType.String, src_p[i]));
						break;
				}
			}
			return result.ToArray();
		}
		private bool ValidateVarName(string name){//Validates name of variable
			if(int.TryParse(name[0].ToString(),out int _)) return false;
			foreach(char letter in name){
				if(!"QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm_".Contains(letter)) return false;
			}
			return true;
		}
		private string InsertVariable(Token token){//Inserts varable values into token's value. reference varable - $nameOfVariable
			List<string> words = new List<string>(token.Value.ToString().Split(' '));
            while (words.Contains("")) words.Remove("");
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
