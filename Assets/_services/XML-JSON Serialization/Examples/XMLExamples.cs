using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class XMLExamples : MonoBehaviour
{

  public class AddressBookEntry
  {
    public string name;
    public int age;
    public bool alreadyMet, married;
    public AddressBookEntry(string n, int a, bool aM, bool m) {name = n; age = a; alreadyMet = aM; married = m;}
    public override string ToString(){ return "(name: " + name + ", age: " + age + ", already-met: " + alreadyMet + ", married: " + married + ")"; }
  }

  void Start()
  {
    ExampleAddressBook();
  }

	void ExampleAddressBook()
  {
    // Let's serialize a simple address book

    XMLOutStream outStream = new XMLOutStream();
    outStream.Start("book")
              .Content("version", 1)
              .Start("entry")
                .Content("name", "Mike")
                .Content("age", 24)
                .Attribute("already-met", true)
                .Attribute("married", true)
              .End()
              .Start("entry")
                .Content("name", "John")
                .Content("age", 32)
                .Attribute("already-met", false)
              .End()
            .End();

    string serialized = outStream.Serialize();

    // serialized outputs this XML structure:
    //
    //
    //    <book>
    //      <entry already-met="true" married=true>
    //        <name>Mike</name>
    //        <age>24</age>
    //      </entry>
    //      <entry already-met="false">
    //        <name>John</name>
    //        <age>32</age>
    //      </entry>
    //    </book>
    //    
    //    

    // Deserialize it

    XMLInStream inStream = new XMLInStream(serialized); // the XML root (here 'book' is automatically entered to parse the content)
    int version;
    List<AddressBookEntry> entries = new List<AddressBookEntry>();
    inStream.Content("version", out version)
            .List("entry", delegate(XMLInStream entryStream){
      string name;
      int age;
      bool alreadyMet;
      bool married = false;
      entryStream.Content("name", out name)
                 .Content("age", out age)
                 .Attribute("already-met", out alreadyMet)
                 .AttributeOptional("married", ref married);
      entries.Add(new AddressBookEntry(name, age, alreadyMet, married));
    });

    // Now version and entries are set

    Debug.Log("SERIALIZED XML STRING: " + serialized);
    string result = "";
    foreach(AddressBookEntry entry in entries)
      result += entry.ToString() + " ";
    Debug.Log("XML DESERIALIZATION of " + entries.Count + " entries: " + result);
  }
}
