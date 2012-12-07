using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using JSONObject = System.Collections.Generic.Dictionary<string, JSON>;
using JSONArray = System.Collections.Generic.List<JSON>;

public enum JSONType {
	NULL,
	OBJECT,
	ARRAY,
	STRING,
	INTEGER,
	FLOAT,
	BOOLEAN
};

public class JSON : IEnumerable {
		
	#region Data Containers
	private JSONObject					object_data;
	private JSONArray 					array_data;
	private string 						string_data;
	private int 						integer_data;
	private float 						float_data;
	private bool 						boolean_data;
	
	public object Value {
		get {
			switch (Type) {
			case JSONType.OBJECT:
				return array_data;
			case JSONType.STRING:
				return string_data;
			case JSONType.INTEGER:
				return integer_data;
			case JSONType.FLOAT:
				return float_data;
			case JSONType.BOOLEAN:
				return boolean_data;
			}
			return null;
		}
	}
	#endregion
	
	#region Type
	JSONType _type;
	
	public JSONType Type {
		get {
			return this._type;
		}
		set {
			switch (value) {
			case JSONType.OBJECT:
				this.object_data = new JSONObject();
				break;
			case JSONType.ARRAY:
				this.array_data = new JSONArray();
				break;
			default:
				break;
			}
			this._type = value;
		}
	}
	#endregion
	
	#region Parser Variables
	JSON parent;
	#endregion
	
	#region Constructors And Parser
	public JSON() {
		Type = JSONType.NULL;
	}
	
	public JSON(JSON p) {
		parent = p;
	}
	
	public JSON (string json) {
		JSON current = this;
		
		for (int pos = 0; pos < json.Length; pos++) {
			switch (json[pos]) {
				
			// Object start
			case '{':
				current.Type = JSONType.OBJECT;
				break;
				
			// Object end
			case '}':
				if (current == this) return;
				UpOneLevel (ref current);
				break;
				
			// Array start
			case '[':
				if (current == this) return;
				if (current.Type == JSONType.NULL) current.Type = JSONType.ARRAY;
				NewArrayElement(ref current);
				break;
				
			// Array end
			case ']':
				UpOneLevel (ref current, false);
				current.array_data.RemoveAt(current.array_data.Count - 1);
				UpOneLevel (ref current);
				break;
				
			// String start
			case '"':
				switch (current.Type) {	
				// String value
				case JSONType.NULL:
					current.Type = JSONType.STRING;
					current.string_data = ParseString(json, ref pos);
					UpOneLevel (ref current);
					break;
				// String key
				case JSONType.OBJECT:
					string key = ParseString(json, ref pos);
					current.object_data[key] = new JSON(current);
					current = current.object_data[key];
					break;
				default:
					throw new FormatException("JSON error: unexpected string");
				}
				break;
			
			// NULL
			case 'n':
				if (json.Substring(pos, 4) == "null") {
					UpOneLevel (ref current);
					pos += 3;
				} else {
					throw new FormatException("JSON error: invalid syntax (did you expect 'null'?)");
				}
				break;
				
			// True
			case 't':
				if (json.Substring(pos, 4) == "true") {
					current.SetValue(true);
					UpOneLevel (ref current);
					pos += 3;
				} else {
					throw new FormatException("JSON error: invalid syntax (did you expect 'true'?)");
				}
				break;
				
			// False
			case 'f':
				if (json.Substring(pos, 5) == "false") {
					current.SetValue(false);
					UpOneLevel (ref current);
					pos += 4;
				} else {
					throw new FormatException("JSON error: invalid syntax (did you expect 'false'?)");
				}
				break;
				
			// Numbers
			case '-':
			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
				if (current.Type != JSONType.NULL)
					throw new InvalidOperationException("JSON error: numbers can be only values");
				int start, end;
				string num_symbols = "01234567890.eE+-";
				start = end = pos;
				while (num_symbols.Contains(json[end].ToString())) {
					end++;
				}
				string number_str = json.Substring(start, end - start);
				try {
					int i = System.Convert.ToInt32(number_str);
					current.SetValue(i);
					UpOneLevel (ref current);
				} catch (Exception) {
					try {
						float f = System.Convert.ToSingle(number_str);
						current.SetValue(f);
						UpOneLevel (ref current);
					} catch (FormatException) {
						throw new FormatException("JSON error: invalid number format");
					}
				}
				pos = end - 1;
				break;
								
			// Whitespaces
			case ' ':
			case '\t':
			case '\n':
			case '\r':
			case '\b':
				break;
				
			case ':':
				break;
			case ',':
				break;
				
			default:
				throw new FormatException("JSON error: unexpected symbol");
			}
		}
	}
	
	public JSON(object obj) {
		Type = JSONType.NULL;
	}
	#endregion
	
	#region Conversions
	static public implicit operator JSON(string s) {
		JSON json = new JSON();
		json.SetValue(s);
		return json;
	}
	
	static public implicit operator JSON(int i) {
		JSON json = new JSON();
		json.SetValue(i);
		return json;
	}
	
	static public implicit operator JSON(float f) {
		JSON json = new JSON();
		json.SetValue(f);
		return json;
	}
	
	static public implicit operator JSON(bool b) {
		JSON json = new JSON();
		json.SetValue(b);
		return json;
	}
	
	static public implicit operator string (JSON json) {
		if(json.Type != JSONType.STRING) throw new InvalidOperationException("JSON error: the instance is not a string");
		return json.string_data;
	}
	
	static public implicit operator int (JSON json) {
		if(json.Type != JSONType.INTEGER && json.Type != JSONType.FLOAT)
			throw new InvalidOperationException("JSON error: the instance is not a number");
		if(json.Type == JSONType.INTEGER) {
			return json.integer_data;
		} else {
			return (int)json.float_data;
		}
	}
	
	static public implicit operator float (JSON json) {
		if(json.Type != JSONType.FLOAT && json.Type != JSONType.INTEGER)
			throw new InvalidOperationException("JSON error: the instance is not a number");
		if(json.Type == JSONType.INTEGER) {
			return json.integer_data;
		} else {
			return json.float_data;
		}
	}
	
	static public implicit operator bool (JSON json) {
		if(json.Type != JSONType.BOOLEAN) throw new InvalidOperationException("JSON error: the instance is not a boolean");
		return json.boolean_data;
	}
	#endregion
	
	#region Accessors
	public JSON this[string key] {
		get {
			if (Type != JSONType.OBJECT)
				throw new InvalidOperationException("Instance must be an object");
			return object_data[key];
		}
		set {
			if (Type == JSONType.NULL) Type = JSONType.OBJECT;
			if (Type != JSONType.OBJECT) throw new InvalidOperationException("Instance must be an object");
			object_data[key] = new JSON(value);
		}
	}
	
	public JSON this[int index] {
		get {
			if (Type != JSONType.ARRAY)
				throw new InvalidOperationException("Instance must be an array");
			return array_data[index];
		}
	}
	
	public void Add(JSON element) {
		if (Type == JSONType.NULL) Type = JSONType.ARRAY;
		if (Type != JSONType.ARRAY)
			throw new InvalidOperationException("Instance must be an array");
		array_data.Add(element);
	}
	
	public void Add(string key, JSON val) {
		if (Type == JSONType.NULL) Type = JSONType.OBJECT;
		if (Type != JSONType.OBJECT)
			throw new InvalidOperationException("Instance must be an object");
		object_data.Add(key, val);
	}
	
	public int Count {
		get {
			return array_data.Count;
		}
	}
	#endregion
	
	#region Setters (internal use only)
	void SetValue (string s) {
		Type = JSONType.STRING;
		string_data = s;
	}
	
	void SetValue (int i) {
		Type = JSONType.INTEGER;
		integer_data = i;
	}
	
	void SetValue (float f) {
		Type = JSONType.FLOAT;
		float_data = f;
	}
	
	void SetValue (bool b) {
		Type = JSONType.BOOLEAN;
		boolean_data = b;
	}
	#endregion
	
	#region Utils
	static string ParseString (string json, ref int pos) {
		int start, end;
		start = pos + 1;
		for (end = start; json[end].ToString() != "\"" || json[end-1].ToString() == "\\"; end++);
		pos = end;
		return json.Substring(start, end - start);
	}
	
	static void UpOneLevel(ref JSON current, bool add_new = true) {
		current = current.parent;
		if ((current.Type == JSONType.ARRAY) && add_new) {
			NewArrayElement(ref current);
		}
	}
	
	static void NewArrayElement(ref JSON current) {
		JSON elm = new JSON(current);
		current.array_data.Add(elm);
		current = elm;
	}
	#endregion
	
	#region Enumerator
	public IEnumerator GetEnumerator() {
		return array_data.GetEnumerator();
	}
	#endregion
	
	#region Serializer
	public override string ToString() {
		string json = "";
		switch(Type) {
		case JSONType.OBJECT:
			foreach (KeyValuePair<string,JSON> item in object_data) {
				json += "\"" + item.Key + "\":" + item.Value.ToString() + ",";
			}
			if (json.Length > 0)
				json = json.Remove(json.Length - 1);
			json = "{" + json + "}";
			break;
		case JSONType.ARRAY:
			foreach (JSON item in array_data) {
				json += item.ToString() + ",";
			}
			if (json.Length > 0)
				json = json.Remove(json.Length - 1);
			json = "[" + json + "]";
			break;
		case JSONType.STRING:
			json = "\"" + string_data + "\"";
			break;
		case JSONType.INTEGER:
		case JSONType.FLOAT:
		case JSONType.BOOLEAN:
			json = System.Convert.ToString(Value).ToLower();
			break;
		case JSONType.NULL:
			json = "null";
			break;
		}
		return json;
	}
	#endregion
};
