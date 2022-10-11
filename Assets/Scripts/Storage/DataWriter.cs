using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataWriter
{
    	BinaryWriter writer;
	
	public DataWriter(BinaryWriter writer)
	{
		this.writer = writer;
	}
	
	public void Write(float value) 
	{
		writer.Write(value);
	}

	public void Write(int value)
	{
		writer.Write(value);
	}

	public void Write(uint value)
	{
		writer.Write(value);
	}
	
	public void Write(bool value)
	{
		writer.Write(value);
	}
	
	public void Write(Quaternion value) 
	{
		writer.Write(value.x);
		writer.Write(value.y);
		writer.Write(value.z);
		writer.Write(value.w);
	}
	
	public void Write(Vector3 value)
	{
		writer.Write(value.x);
		writer.Write(value.y);
		writer.Write(value.z);
	}
	
	public void Write(Vector2 value)
	{
		writer.Write(value.x);
		writer.Write(value.y);
	}
	
	public void Write(string value)
	{
		writer.Write(value.Length);
		writer.Write(Cipher.GetAndSet());
		char[] chars = value.ToCharArray();
		foreach(char c in chars){
			writer.Write(Cipher.Encrypt((uint)c));
		}
	}
	
	//arrays
	public void Write(float[] values)
	{
		writer.Write(values.Length);
		foreach(float f in values){
			writer.Write(f);
		}
	}
	
	public void Write(int[] values)
	{
		writer.Write(values.Length);
		foreach(int n in values){
			writer.Write(n);
		}
	}
	
	public void Write(bool[] values)
	{
		writer.Write(values.Length);
		foreach(bool b in values){
			writer.Write(b);
		}
	}
	
	public void Write(Quaternion[] values)
	{
		writer.Write(values.Length);
		foreach(Quaternion q in values){
			Write(q);
		}
	}
	
	public void Write(Vector3[] values)
	{
		writer.Write(values.Length);
		foreach(Vector3 v3 in values){
			Write(v3);
		}
	}
	
	public void Write(Vector2[] values)
	{
		writer.Write(values.Length);
		foreach(Vector2 v2 in values){
			Write(v2);
		}
	}
	
	public void Write(string[] values)
	{
		writer.Write(values.Length);
		writer.Write(Cipher.GetAndSet());
		foreach(string s in values){
			char[] chars = s.ToCharArray();
			writer.Write(chars.Length);
			foreach(char c in chars){
				writer.Write(Cipher.Encrypt(c));
			}
		}
	}
}
