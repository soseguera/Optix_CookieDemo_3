#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.Retentivity;
using FTOptix.NativeUI;
using FTOptix.NetLogic;
using FTOptix.UI;
using FTOptix.Recipe;
using FTOptix.Core;
using FTOptix.CoreBase;
using FTOptix.SQLiteStore;
using FTOptix.Store;
using FTOptix.ODBCStore;
using FTOptix.WebUI;
using FTOptix.MicroController;
#endregion

public class RecipesEditorComboBoxLogic1 : BaseNetLogic
{
	public override void Start()
	{
		comboBox = (ComboBox)Owner;
		comboBox.SelectedValueVariable.VariableChange += SelectedValueVariable_VariableChange;
		LoadSelectedRecipeData();
		Log.Info("Second Step ");
	}

	private void SelectedValueVariable_VariableChange(object sender, VariableChangeEventArgs e)
	{
		LoadSelectedRecipeData();
	}

	private void LoadSelectedRecipeData()
	{
		LocalizedText selectedText = (LocalizedText)comboBox.SelectedValue;
		if (selectedText == null || string.IsNullOrEmpty(selectedText.Text))
		{
			Log.Info("empty 01 Step ");
			return;
		}
			

		var recipeSchemaEditor = Owner.Owner;
		var recipeSchemaVariable = recipeSchemaEditor.GetVariable("RecipeSchema");
		if (recipeSchemaVariable == null)
		{
			Log.Info("empty 02 Step ");
			return;
		}
			
		
		var recipeSchemaNodeId = (NodeId)recipeSchemaVariable.Value.Value;

		var recipeSchemaObject = (RecipeSchema)InformationModel.Get(recipeSchemaNodeId);
		if (recipeSchemaObject == null)
		{
			Log.Info("empty 03 Step ");
			return;
		}

		var editModelNode = recipeSchemaObject.GetObject("EditModel");
		if (editModelNode == null)
		{
			Log.Info("empty 04 Step ");
			return;
		}

		recipeSchemaObject.CopyFromStoreRecipe(comboBox.Text, editModelNode.NodeId, CopyErrorPolicy.BestEffortCopy);
		Log.Info("First Step ");
	}

	public override void Stop()
	{
		comboBox.SelectedValueVariable.VariableChange -= SelectedValueVariable_VariableChange;
	}

	private ComboBox comboBox;
}
