using UnityEngine;
using UnityEditor;
 
using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
 
public class SelectByComponent : ScriptableWizard {
 
	public String m_Component;
 
	private List< Type > m_Types = new List< Type >();
 
 
    [MenuItem ("Selection/Select by component")]
    static void static_SelectByComponent() { 
 
    	ScriptableWizard.DisplayWizard("Select by component", typeof(SelectByComponent), "Select");
    }
 
    public SelectByComponent()
    {
    	FillTypeList();
    }
 
    void OnWizardCreate()
    {
 
		if ( m_Component == "" )
			return;
 
		Type t = GetSelectedType();
 
		if ( t == null )
			return;
 
		List< GameObject > gos = new List< GameObject >();
 
		foreach ( UnityEngine.Object obj in UnityEngine.Object.FindObjectsOfType( t ) )
		{
 
			Component c = obj as Component;
 
			if ( c!= null )
			{
				gos.Add( c.gameObject );	
			}
 
		}
 
		Selection.objects = gos.ToArray();
 
    }
 
    Type GetSelectedType()
    {
  		foreach ( Type t in m_Types )
		{
			if ( t.Name == m_Component )
				return t;	
		}  	
		return null;
    }
 
    void OnWizardUpdate()
    {
 
    	helpString = "Enter a Component type name (i.e Rigidbody). Type must inherit from Component";
 
    	errorString = "";
 
		Type t = GetSelectedType();
 
		if ( t == null )
			errorString = "Type doesnt exist.";
 
		if ( m_Types.Count == 0 )
			errorString = "Typelist is empty.";
 
    }	
 
    void FillTypeList()
    {
     	AppDomain domain = AppDomain.CurrentDomain;
 
    	Type ComponentType = typeof( Component );
 
    	m_Types.Clear();
 
    	foreach ( Assembly asm in domain.GetAssemblies() )
    	{
 
    		Assembly currentAssembly = null; 
 
    		//	add UnityEngine.dll component types
    		if ( asm.FullName == "UnityEngine" )	
    			currentAssembly = asm;
 
    		//	check only for temporary assemblies (i.e. d6a5e78fb39c28ds27a1ec4f9g1 )
    		if ( ContainsNumbers( asm.FullName ) )
    			currentAssembly = asm;
 
    		if ( currentAssembly != null )
    		{
    			foreach ( Type t in currentAssembly.GetExportedTypes() )
    			{
    				if ( ComponentType.IsAssignableFrom( t ) )
    				{
    					m_Types.Add( t );
    				}
    			}
    		}
 
    	}   	
    }
 
    bool ContainsNumbers( String text )
    {
 
		int i = 0;
    	foreach ( char c in text )
    	{
    		if ( int.TryParse( c.ToString(), out i ) )
    			return true;
    	}	
 
		return false;
 
    }
 
 
}