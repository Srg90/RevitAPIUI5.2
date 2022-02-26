using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Prism.Commands;
using RevitAPIUILibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Plumbing;

namespace RevitAPIUI2
{
    public class MainViewViewModel
    {
        private ExternalCommandData _commandData;

        public DelegateCommand SaveCommand { get; }
        public List<Element> PickedObjects { get; } = new List<Element>();
        public List<WallType> WallSystems { get; } = new List<WallType>();
        public WallType SelectedWallSystem { get; set; }

        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            SaveCommand = new DelegateCommand(OnSaveCommand);
            PickedObjects = SelectionUtils.PickObjects(commandData);
            WallSystems = PipesUtils.GetPipingSystems(commandData);
        }

        private void OnSaveCommand()
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            if (PickedObjects.Count == 0 || SelectedWallSystem == null)
                return;

            using (var ts = new Transaction(doc, "Set System Type"))
            {
                ts.Start();
                
                foreach (var pickedObject in PickedObjects)
                {
                    if (pickedObject is Wall)
                    {
                        var oWall = pickedObject as Wall;
                        oWall.ChangeTypeId(SelectedWallSystem.Id);
                    }
                }

                ts.Commit();
            }

            RaiseCloseRequest();
        }

        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}
