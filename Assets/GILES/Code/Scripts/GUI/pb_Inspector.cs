using System;
using UnityEngine;
using System.Reflection;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections.Generic;
using GILES.Serialization;

namespace GILES.Interface
{
	/**
	 * A generic gameobject inspector, with support for editing basic types.
	 */
	public class pb_Inspector : MonoBehaviour
	{
		/// A reference to the inspector GUI scroll panel.  All new UI elements will be 
		/// instantiated as children of this GameObject.
		public GameObject inspectorScrollPanel;

		/// The currently inspected gameobject.
		private GameObject currentSelection;

		/// By default don't show the Unity components.
		public bool showUnityComponents = false;

		/// A cache of the currently active component editors in the inspector.
		private List<pb_ComponentEditor> componentEditors = new List<pb_ComponentEditor>();

		void Start()
		{
			pb_Selection.AddOnSelectionChangeListener(OnSelectionChange);
			Undo.AddUndoPerformedListener( UndoRedoPerformed );
			Undo.AddRedoPerformedListener( UndoRedoPerformed );
		}

		void UndoRedoPerformed()
		{
			foreach(pb_ComponentEditor editor in componentEditors)
			{
				editor.UpdateGUI();
			}
		}

		void OnSelectionChange(IEnumerable<GameObject> selection)
		{
			// build inspector queue
			if(currentSelection != pb_Selection.activeGameObject)
			{
				RebuildInspector( pb_Selection.activeGameObject );
				currentSelection = pb_Selection.activeGameObject;
			}
		}

		public void RebuildInspector(GameObject go)
		{
			ClearInspector();

			if(go == null)
				return;

			foreach(Component component in go.GetComponents<Component>())
			{
				if(	component == null ||
					pb_Reflection.HasIgnoredAttribute(component.GetType()) ||
					System.Attribute.GetCustomAttribute(component.GetType(), typeof(pb_InspectorIgnoreAttribute)) != null ||
					(!showUnityComponents && pb_Config.IgnoredComponentsInInspector.Contains(component.GetType())))
					continue;

				GameObject panel = pb_GUIUtility.CreateLabeledVerticalPanel(component.GetType().ToString());
				panel.transform.SetParent(inspectorScrollPanel.transform);

				pb_ComponentEditor inspector = null;

				if( typeof(pb_ICustomEditor).IsAssignableFrom(component.GetType()) )
					inspector = ((pb_ICustomEditor)component).InstantiateInspector(component);
				else
					inspector = pb_ComponentEditorResolver.GetEditor(component);

				inspector.transform.SetParent(panel.transform);

				componentEditors.Add(inspector);
			}
		}

		void ClearInspector()
		{
			foreach(Transform go in inspectorScrollPanel.transform)
				pb_ObjectUtility.Destroy(go.gameObject);

			componentEditors.Clear();
		}

		/**
		 * Callback for visibility toggle.
		 */
		public void ToggleInspector(bool show)
		{
			// GetComponent<RectTransform>().bottom = show ? 0f : 200f;
		}
	}
}