﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace UnityEngine.InputSystem.Editor
{
	internal class CompositePartBindingPropertiesView : UIToolkitView<CompositePartBindingPropertiesView.ViewState>
	{
		private readonly VisualElement m_Root;
		private readonly DropdownField m_CompositePartField;
		private readonly IMGUIContainer m_PathEditorContainer;

		private const string UxmlName = GlobalInputActionsConstants.PackagePath +
		                                GlobalInputActionsConstants.ResourcesPath +
		                                GlobalInputActionsConstants.CompositePartBindingPropertiesViewUxml;

		public CompositePartBindingPropertiesView(VisualElement root, StateContainer stateContainer)
			: base(stateContainer)
		{
			m_Root = root;
			var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlName);
			var container = visualTreeAsset.CloneTree();
			m_Root.Add(container);

			m_PathEditorContainer = container.Q<IMGUIContainer>("path-editor-container");
			m_CompositePartField = container.Q<DropdownField>("composite-part-dropdown");

			CreateSelector(Selectors.GetSelectedBinding,
				Selectors.GetCompositePartBindingViewState);
		}

		public override void RedrawUI(ViewState viewState)
		{
			// TODO: Persist control picker state
			var controlPathEditor = new InputControlPathEditor(viewState.selectedBindingPath, new InputControlPickerState(),
				() => { /* the InputControlPathEditor already updates the serialized object internally, so we have nothing to do here */ });

			controlPathEditor.SetExpectedControlLayout(viewState.expectedControlLayoutName);
			
			m_PathEditorContainer.onGUIHandler = controlPathEditor.OnGUI;
			
			m_CompositePartField.choices.Clear();
			m_CompositePartField.choices.AddRange(viewState.compositePartNames);
			m_CompositePartField.SetValueWithoutNotify(viewState.selectedCompositePartName);

			m_CompositePartField.RegisterValueChangedCallback(evt =>
			{
				Dispatch(Commands.SetCompositeBindingPartName(viewState.selectedBinding, evt.newValue));
			});
		}

		internal class ViewState
		{
			public SerializedProperty selectedBindingPath;
			public SerializedInputBinding selectedBinding;
			public IEnumerable<string> compositePartNames;
			public string expectedControlLayoutName;
			public string selectedCompositePartName;
		}
	}

	internal static partial class Selectors
	{
		public static CompositePartBindingPropertiesView.ViewState GetCompositePartBindingViewState(SerializedInputBinding binding,
			GlobalInputActionsEditorState state)
		{
			var compositeParts = GetCompositePartOptions(binding.name, binding.compositePath).ToList();
			var selectedCompositePartName = ObjectNames.NicifyVariableName(
				compositeParts.First(str => string.Equals(str, binding.name, StringComparison.OrdinalIgnoreCase)));

			var compositePartBindingViewState = new CompositePartBindingPropertiesView.ViewState
			{
				selectedBinding = binding,
				selectedBindingPath = GetSelectedBindingPath(state),
				selectedCompositePartName = selectedCompositePartName,
				compositePartNames = compositeParts.Select(ObjectNames.NicifyVariableName).ToList(),
				expectedControlLayoutName = InputBindingComposite.GetExpectedControlLayoutName(binding.compositePath, binding.name) ?? ""
			};
			return compositePartBindingViewState;
		}
	}
}