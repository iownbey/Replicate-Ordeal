using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataReader
{
   	BinaryReader reader;

	public DataReader(BinaryReader reader) 
	{
		this.reader = reader;
	}
	
	public float ReadFloat() 
	{
		return reader.ReadSingle();
	}

	public int ReadInt() 
	{
		return reader.ReadInt32();
	}
	
	public bool ReadBool()
	{
		return reader.ReadBoolean();
	}
	
	public Quaternion ReadQuaternion() 
	{
		Quaternion value;
		value.x = reader.ReadSingle();
		value.y = reader.ReadSingle();
		value.z = reader.ReadSingle();
		value.w = reader.ReadSingle();
		return value;
	}

	public Vector3 ReadVector3() 
	{
		Vector3 value;
		value.x = reader.ReadSingle();
		value.y = reader.ReadSingle();
		value.z = reader.ReadSingle();
		return value;
	}
	
	public Vector2 ReadVector2()
	{
		Vector2 value;
		value.x = reader.ReadSingle();
		value.y = reader.ReadSingle();
		return value;
	}
	
	public string ReadString()
	{
		int length = reader.ReadInt32();
		Cipher.SetState(reader.ReadUInt32());
		string result = "";
		for(int i = 0; i < length; i ++){
			result += (char) Cipher.Decrypt(reader.ReadUInt32());
		}
		return result;
	}
	
	//arrays and Lists
	public float[] ReadFloatArray()
	{
		int length = reader.ReadInt32();
		float[] output = new float[length];
		for(int i = 0; i < length; i ++){
			float f = reader.ReadSingle();
			output[i] = f;
		}
		return output;
	}
	
	public int[] ReadIntArray()
	{
		int length = reader.ReadInt32();
		int[] output = new int[length];
		for(int i = 0; i < length; i ++){
			int n = reader.ReadInt32();
			output[i] = n;
		}
		return output;
	}
	
	public bool[] ReadBoolArray()
	{
		int length = reader.ReadInt32();
		bool[] output = new bool[length];
		for(int i = 0; i < length; i ++){
			bool b = reader.ReadBoolean();
			output[i] = b;
		}
		return output;
	}
	
	public Quaternion[] ReadQuaternionArray()
	{
		int length = reader.ReadInt32();
		Quaternion[] output = new Quaternion[length];
		for(int i = 0; i < length; i ++){
			Quaternion q = ReadQuaternion();
			output[i] = q;
		}
		return output;
	}
	
	public Vector3[] ReadVector3Array()
	{
		int length = reader.ReadInt32();
		Vector3[] output = new Vector3[length];
		for(int i = 0; i < length; i ++){
			Vector3 v = ReadVector3();
			output[i] = v;
		}
		return output;
	}
	
	public Vector2[] ReadVector2Array()
	{
		int length = reader.ReadInt32();
		Vector2[] output = new Vector2[length];
		for(int i = 0; i < length; i ++){
			Vector2 v = ReadVector2();
			output[i] = v;
		}
		return output;
	}
	
	public string[] ReadStringArray()
	{
		int length = reader.ReadInt32();
		string[] output = new string[length]; 
		Cipher.SetState(reader.ReadUInt32());
		for(int i = 0; i < length; i ++){
			string s = "";
			int sl = reader.ReadInt32();
			for(int j = 0; j < sl; j ++){
				s += (char) Cipher.Decrypt(reader.ReadUInt32());
			}
			output[i] = s;
		}
		return output;
	}
}
