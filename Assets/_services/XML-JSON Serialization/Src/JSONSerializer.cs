using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

public static class JSONSerializer
{
  //---------------------------------------------------------------------------------
  // static Deserialize
  //---------------------------------------------------------------------------------
  public static T Deserialize<T>(string message) where T: new()
  {
    #if UNITY_FLASH && !UNITY_EDITOR
      Debug.LogError("JSONSerializer do not work with Flash !");
      return default(T);
    #else
      JSONInStream stream = new JSONInStream(message);
      return (T) DeserializeObject(stream, typeof(T));
    #endif
  }

  //---------------------------------------------------------------------------------
  // static Serialize
  //---------------------------------------------------------------------------------
  public static string Serialize<T>(T message)
  {
    #if UNITY_FLASH && !UNITY_EDITOR
      Debug.LogError("JSONSerializer do not work with Flash !");
      return null;
    #else
      JSONOutStream stream = new JSONOutStream();
      SerializeObject(stream, typeof(T), message);
      return stream.Serialize();
    #endif
  }

#if UNITY_FLASH && !UNITY_EDITOR
#else

  //---------------------------------------------------------------------------------
  // static SerializeObject
  //---------------------------------------------------------------------------------
  private static void SerializeObject(JSONOutStream stream, Type type, object message)
  {
    MethodInfo toJSON = type.GetMethod("ToJSON");
    if(toJSON != null)
    {
      toJSON.Invoke(message, new object[]{stream});
      return;
    }
    System.Reflection.FieldInfo[] fieldInfo = type.GetFields();
    foreach (System.Reflection.FieldInfo info in fieldInfo)
    {
      switch(info.FieldType.ToString())
      {
      case "System.String":
        stream.Content(info.Name, (string) info.GetValue(message));
        break;
      case "System.Single":
        stream.Content(info.Name, (float) info.GetValue(message));
        break;
      case "System.Double":
        stream.Content(info.Name, (double) info.GetValue(message));
        break;
      case "System.Int32":
        stream.Content(info.Name, (int) info.GetValue(message));
        break;
      case "System.Boolean":
        stream.Content(info.Name, (bool) info.GetValue(message));
        break;
      case "UnityEngine.Vector3":
        stream.Content(info.Name, (Vector3) info.GetValue(message));
        break;
      case "UnityEngine.Quaternion":
        stream.Content(info.Name, (Quaternion) info.GetValue(message));
        break;
      case "UnityEngine.Color":
        stream.Content(info.Name, (Color) info.GetValue(message));
        break;
      case "UnityEngine.Rect":
        stream.Content(info.Name, (Rect) info.GetValue(message));
        break;
      case "UnityEngine.Vector2":
        stream.Content(info.Name, (Vector2) info.GetValue(message));
        break;
      default:
        if(info.FieldType.IsEnum) // Enum
        {
          stream.Content(info.Name, (string) info.GetValue(message).ToString());
        }
        else if(info.FieldType.IsGenericType) // List
        {
          Type containedType = info.FieldType.GetGenericArguments()[0];
          Type typeList = typeof(List<>);
          Type actualType = typeList.MakeGenericType(containedType);
          PropertyInfo countMethod = actualType.GetProperty("Count");
          PropertyInfo itemMethod = actualType.GetProperty("Item");
          int count = (int) countMethod.GetValue(info.GetValue(message), new object[]{});
          stream.List(info.Name);
          for(int i = 0; i < count ; ++i)
          {
            object o = itemMethod.GetValue(info.GetValue(message), new object[]{i});
            SerializeListElement(stream, containedType, o, i);
          }
          stream.End();
        }
        else if(info.FieldType.IsArray) // Array
        {
          object[] content = ToObjectArray((IEnumerable) info.GetValue(message));
          Type containedType = Type.GetTypeArray(content)[0];
          stream.List(info.Name);
          for(int i = 0; i < content.Length ; ++i)
          {
            object o = content[i];
            SerializeListElement(stream, containedType, o, i);
          }
          stream.End();
        }
        else // object
        {
          stream.Start(info.Name);
          SerializeObject(stream, info.FieldType, info.GetValue(message));
          stream.End();
        }
        break;
      }
    }
  }

  //---------------------------------------------------------------------------------
  // SerializeListElement
  //---------------------------------------------------------------------------------
  private static void SerializeListElement(JSONOutStream stream, Type type, object message, int i)
  {
      switch(type.ToString())
      {
      case "System.String":
        stream.Content(i, (string) message);
        break;
      case "System.Single":
        stream.Content(i, (float) message);
        break;
      case "System.Double":
        stream.Content(i, (double) message);
        break;
      case "System.Int32":
        stream.Content(i, (int) message);
        break;
      case "System.Boolean":
        stream.Content(i, (bool) message);
        break;
      case "UnityEngine.Vector3":
        stream.Content(i, (Vector3) message);
        break;
      case "UnityEngine.Quaternion":
        stream.Content(i, (Quaternion) message);
        break;
      case "UnityEngine.Color":
        stream.Content(i, (Color) message);
        break;
      case "UnityEngine.Rect":
        stream.Content(i, (Rect) message);
        break;
      case "UnityEngine.Vector2":
        stream.Content(i, (Vector2) message);
        break;
      default:
        stream.Start(i);
        SerializeObject(stream, type, message);
        stream.End();
        break;
      }
  }

  //---------------------------------------------------------------------------------
  //  DeserializeObject
  //---------------------------------------------------------------------------------
  private static object DeserializeObject(JSONInStream stream, Type type)
  {
    MethodInfo fromJSON = type.GetMethod("FromJSON");
    if(fromJSON != null)
    {
      return fromJSON.Invoke(null, new object[]{stream});
    }

    object data = Activator.CreateInstance(type);
    System.Reflection.FieldInfo[] fieldInfo = type.GetFields();
    foreach (System.Reflection.FieldInfo info in fieldInfo)
    {
      switch(info.FieldType.ToString())
      {
      case "System.String":
        string resS;
        stream.Content(info.Name, out resS);
        info.SetValue(data, resS);
        break;
      case "System.Single":
        float resF;
        stream.Content(info.Name, out resF);
        info.SetValue(data, resF);
        break;
      case "System.Double":
		double resD;
        stream.Content(info.Name, out resD);
		info.SetValue(data, resD);
        break;
      case "System.Int32":
        int resI;
        stream.Content(info.Name, out resI);
        info.SetValue(data, resI);
        break;
      case "System.Boolean":
        bool resB;
        stream.Content(info.Name, out resB);
        info.SetValue(data, resB);
        break;
      case "UnityEngine.Vector3":
        Vector3 resV;
        stream.Content(info.Name, out resV);
        info.SetValue(data, resV);
        break;
      case "UnityEngine.Quaternion":
        Quaternion resQ;
        stream.Content(info.Name, out resQ);
        info.SetValue(data, resQ);
        break;
      case "UnityEngine.Color":
        Color resC;
        stream.Content(info.Name, out resC);
        info.SetValue(data, resC);
        break;
      case "UnityEngine.Rect":
        Rect resR;
        stream.Content(info.Name, out resR);
        info.SetValue(data, resR);
        break;
      case "UnityEngine.Vector2":
        Vector2 resV2;
        stream.Content(info.Name, out resV2);
        info.SetValue(data, resV2);
        break;
      default:
        if(info.FieldType.IsEnum) // Enum
        {
          string e;
          stream.Content(info.Name, out e);
          info.SetValue(data, Enum.Parse(info.FieldType, e));
        }
        else if(info.FieldType.IsGenericType) // can only be a List
        {
          Type containedType = info.FieldType.GetGenericArguments()[0];
          Type typeList = typeof(List<>);
          Type actualType = typeList.MakeGenericType(containedType);
          MethodInfo addMethod = actualType.GetMethod("Add");
          object list = Activator.CreateInstance(actualType);
          stream.List(info.Name, delegate(int i, JSONInStream stream2){
            object o = DeserializeListElement(stream2, containedType);
            addMethod.Invoke(list, new object[]{o});
          });
          info.SetValue(data, list);
        }
        else if(info.FieldType.IsArray) // Array
        {
          Type containedType = info.FieldType.GetElementType();
          Type typeList = typeof(List<>);
          Type actualType = typeList.MakeGenericType(containedType);
          MethodInfo addMethod = actualType.GetMethod("Add");
          MethodInfo toArrayMethod = actualType.GetMethod("ToArray");
          object list = Activator.CreateInstance(actualType);
          stream.List(info.Name, delegate(int i, JSONInStream stream2){
            object o = DeserializeListElement(stream2, containedType);
            addMethod.Invoke(list, new object[]{o});
          });
          object array = toArrayMethod.Invoke(list, new object[]{});
          info.SetValue(data, array);
        }
        else // Embedded Object
        {
          stream.Start(info.Name);
          object created = DeserializeObject(stream, info.FieldType);
          stream.End();
          info.SetValue(data, created);
        }
        break;
      }
    }
    return data;
  }

  // Useful
  static object[] ToObjectArray(IEnumerable enumerableObject)
  {
    List<object> oList = new List<object>();
    foreach (object item in enumerableObject) { oList.Add(item); }
    return oList.ToArray();
  }

  //---------------------------------------------------------------------------------
  //  DeserializeListElement
  //---------------------------------------------------------------------------------
  private static object DeserializeListElement(JSONInStream stream, Type type)
  {
      switch(type.ToString())
      {
      case "System.String":
        string resS;
        stream.Content(0, out resS);
        return resS;
      case "System.Single":
        float resF;
        stream.Content(0, out resF);
        return resF;
      case "System.Double":
        double resD;
        stream.Content(0, out resD);
        return resD;
      case "System.Int32":
        int resI;
        stream.Content(0, out resI);
        return resI;
      case "System.Boolean":
        bool resB;
        stream.Content(0, out resB);
        return resB;
      case "UnityEngine.Vector3":
        Vector3 resV;
        stream.Content(0, out resV);
        return resV;
      case "UnityEngine.Quaternion":
        Quaternion resQ;
        stream.Content(0, out resQ);
        return resQ;
      case "UnityEngine.Color":
        Color resC;
        stream.Content(0, out resC);
        return resC;
      case "UnityEngine.Rect":
        Rect resR;
        stream.Content(0, out resR);
        return resR;
      case "UnityEngine.Vector2":
        Vector2 resV2;
        stream.Content(0, out resV2);
        return resV2;
      default:
        stream.Start(0);
        object created = DeserializeObject(stream, type);
        stream.End();
        return created;
      }
  }
#endif
}
