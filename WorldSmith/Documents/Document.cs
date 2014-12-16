﻿using DigitalRune.Windows.Docking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorldSmith.Panels;

namespace WorldSmith.Documents
{
  

    public enum DocumentSource
    {
        File,
        VPK,
    }

    public delegate void DocumentEventHandler(IEditor source);


    /// <summary>
    /// Base class for document types. 
    /// 
    /// These are documents that exist and can be saved (or not, depending if they come from the VPK)
    /// 
    /// </summary>
    public abstract class Document
    {
        public string Name
        {
            get;
            set;
        }

        public bool IsEdited
        {
            get;
            set;
        }

        public string Path
        {
            get;
            set;
        }

        public DocumentSource Source
        {
            get;
            set;
        }

        public bool ReadOnly
        {
            get;
            set;
        }

        private List<IEditor> AttachedEditors = new List<IEditor>();


        /// <summary>
        /// Event that is raised when the document is changed and should be saved
        /// </summary>
        public event DocumentEventHandler OnDocumentEdited;

        /// <summary>
        /// Event that is raised when the document is saved and should be reloaded by all editors. 
        /// </summary>
        public event DocumentEventHandler OnDocumentSaved;


        protected abstract void DoSave();

        public virtual void Save(IEditor source)
        {
            if (!IsEdited) return; //Don't bother saving if we haven't edited this document


            DoSave();
            if(OnDocumentSaved == null)
            {
                Console.WriteLine("[Warning] Received Notification that" + Name + " was saved but we have no save handlers!");
                return;
            }
            OnDocumentSaved(source);
        }

        public abstract void Reload();

        public abstract bool CanEditorOpen(Type EditorType);

        public abstract void OpenDefaultEditor();

        public void DocumentEdited(IEditor source)
        {
            if(OnDocumentEdited == null)
            {
                Console.WriteLine("[Warning] Received Notification that"+ Name +" was edited but we have no edited handlers!");
                return;
            }
            OnDocumentEdited(source);

        }

        /// <summary>
        /// Creates an editor of the given type, or if one is already open, brings it to focus
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T OpenEditor<T>() where T : IEditor
        {
            T Editor = (T)AttachedEditors.FirstOrDefault(x => x.GetType() == typeof(T));
            if(Editor != null)
            {
                DockableForm form = Editor as DockableForm;
                if(form != null)
                {
                    //Bring this form to the forefront in the docking area it is already at. 
                    form.Show(MainForm.PrimaryDockingPanel, form.DockState); 
                }
                

                return Editor;
            }

            //Create the new editor
            Editor = (T)typeof(T).GetConstructor(Type.EmptyTypes).Invoke(new object[] { });
            Editor.ActiveDocument = this;

            OnDocumentEdited += Editor.NotifyDocumentModified;
            OnDocumentSaved += Editor.NotifyDocumentSaved;

            AttachedEditors.Add(Editor);

            return Editor;
        }
        /// <summary>
        /// Get an editor of the given type. Returns null if editor does not exist
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T GetEditor<T>() where T : IEditor
        {
            T Editor = (T)AttachedEditors.FirstOrDefault(x => x.GetType() == typeof(T));
            return Editor;
        }

        public virtual bool ContainsEditor<T>() where T : IEditor
        {           
             return GetEditor<T>() != null;
        }

    }
}
