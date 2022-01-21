﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace GLFrameworkEngine
{
    public partial class GLScene
    {
        public List<EditableObject> GetEditObjects()
        {
            List<EditableObject> objects = new List<EditableObject>();
            foreach (var ob in this.Objects)
            {
                if (ob is EditableObject && ((EditableObject)ob).IsSelected)
                    objects.Add((EditableObject)ob);
            }
            return objects;
        }

        public void OnKeyDown(KeyEventInfo e, GLContext context)
        {
            foreach (IDrawableInput ob in Objects.Where(x => x is IDrawableInput))
                ob.OnKeyDown();
        }

        public void DeleteSelected() {
            var selected = GetEditObjects();
            AddToUndo(new EditableObjectDeletedUndo(this, selected));
            //Remove edit object types
            foreach (var ob in selected) {
                this.RemoveRenderObject(ob);
            }
            //Remove path types
            foreach (var ob in this.Objects)
            {
                if (ob is RenderablePath && ((RenderablePath)ob).EditMode)
                    ((RenderablePath)ob).RemoveSelected();
            }
            GLContext.ActiveContext.UpdateViewport = true;
        }

        List<EditableObject> CopiedObjects = new List<EditableObject>();

        public void CopySelected() {
            CopiedObjects.Clear();

            var selected = GetEditObjects();

            foreach (EditableObject obj in selected)
            {
                if (obj is ICloneable)
                    CopiedObjects.Add(obj);
            }
        }

        public void PasteSelected(GLContext context)
        {
            if (CopiedObjects.Count > 0)
                GLContext.ActiveContext.Scene.AddToUndo(new EditableObjectAddUndo(this, CopiedObjects));

            DeselectAll(context);
            foreach (var obj in CopiedObjects)
            {
                EditableObject copy = ((ICloneable)obj).Clone() as EditableObject;
                AddRenderObject(copy);

                copy.IsSelected = true;
            }
            OnSelectionChanged(context);
        }

        /// <summary>
        /// Toggles edit mode for an editable part object.
        /// </summary>
        public void ToggleEditMode()
        {
            foreach (var obj in this.GetSelectableObjects())
            {
                if (obj is IEditModeObject)
                {
                    bool editMode = ((IEditModeObject)obj).EditMode;
                    if (!editMode && obj.IsSelected)
                    {
                        EditModeObjects.Add(obj);
                        ((IEditModeObject)obj).EditMode = true;
                    }
                    else if (editMode)
                    {
                        obj.IsSelected = true;
                        EditModeObjects.Remove(obj);
                        ((IEditModeObject)obj).EditMode = false;
                    }
                }
            }
            GLContext.ActiveContext.TransformTools.InitAction(GetSelected());
        }
    }
}
