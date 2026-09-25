namespace CodeBase.Data
{
  public static class DataExtensions
  {
    public static string ToJson(this object obj) => 
      UnityEngine.JsonUtility.ToJson(obj);

    public static T ToDeserialized<T>(this string json) =>
      UnityEngine.JsonUtility.FromJson<T>(json);
  }
}
