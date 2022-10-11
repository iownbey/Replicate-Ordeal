public interface ISavable
{
    void Save(DataWriter writer);
	
	void Load(DataReader reader);
}
