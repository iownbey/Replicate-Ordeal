using System.IO;
using UnityEngine;

public class Storage
{
    string savePath;
	public string FileName{get; private set;}
	
	public void Save (ISavable s)
	{
		using (var writer = new BinaryWriter(File.Open(savePath, FileMode.Create))) 
		{
			s.Save(new DataWriter(writer));
		}
	}

	public void Load (ISavable s)
	{
		using (var reader = new BinaryReader(File.Open(savePath, FileMode.Open))) 
		{
			s.Load(new DataReader(reader));
		}
	}
	
	public bool SafeLoad(ISavable s)
	{
		if(File.Exists(savePath)){
			using (var reader = new BinaryReader(File.Open(savePath, FileMode.Open))) 
			{
				s.Load(new DataReader(reader));
				return true;
			}
		}
		return false;
	}
	
	public void SetPath(string pathName)
	{
		savePath = Path.Combine(Application.persistentDataPath, pathName+".savefile");
		FileName = pathName;
		Debug.Log(savePath);
	}
	
	//Constructor
	public Storage(string newPath)
	{
		savePath = Path.Combine(Application.persistentDataPath, newPath+".savefile");
		FileName = newPath;
	}
}
