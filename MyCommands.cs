using System;
using System.Activities;
using System.AddIn;
using System.Configuration;
using System.CodeDom;
using System.Text;
using System.Linq;
using System.Xml;
using System.Reflection;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Windows;

namespace MoveBlockAddin
{
    public class MyCommands
    {
        [CommandMethod("DrawLineAndMoveCircle")]
        public static void DrawLineAndMoveCircle()
        {
            // Get the current document and database
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor edt = doc.Editor;

            //Select Starting point
            PromptPointOptions ppo = new PromptPointOptions("Pick starting point: ");
            PromptPointResult ppr = edt.GetPoint(ppo);
            Point3d startPt = ppr.Value;

            //Select the end point
            ppo = new PromptPointOptions("Pick end point");
            ppo.UseBasePoint = true;
            ppo.BasePoint = startPt;
            ppr = edt.GetPoint(ppo);
            Point3d endPt = ppr.Value;

            if (startPt == null || endPt == null)
            {
                edt.WriteMessage("Invalid point");
                return;
            }

            //Start a transaction
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //Open the block table for read
                BlockTable bt;
                bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

                //Open the Block table record Model space for write
                BlockTableRecord btr;
                btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Construct the line based on the 2 points above (In data Base but not in the drawing)
                Line ln = new Line(startPt, endPt);
                ln.SetDatabaseDefaults();

                // Add the line to the drawing
                btr.AppendEntity(ln);
                trans.AddNewlyCreatedDBObject(ln, true);

                // Construct circle in starting point

                Circle C1 = new Circle();
                C1.Center = startPt;
                C1.Radius = 10;
                C1.SetDatabaseDefaults();

                // Add the circle to the drawing
                btr.AppendEntity(C1);
                trans.AddNewlyCreatedDBObject(C1, true);

                // Create a matrix and move the circle using a vector
                Vector3d destVector = startPt.GetVectorTo(endPt);

                C1.TransformBy(Matrix3d.Displacement(destVector));

                // Commit the transaction
                trans.Commit();

            }
        }

        // Command to Get Block in drawing and move it
        [CommandMethod("GetObjectAndMove")]
        public static void GetObjectAndMove()
        {
            // Get the current document and database
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor edt = doc.Editor;

            //Start a transaction
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // Request for objects to be selected in the drawing area
                PromptSelectionResult acSSPrompt = doc.Editor.GetSelection();

                // If the prompt status is OK, objects were selected
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    SelectionSet acSSet = acSSPrompt.Value;
                    // Step through the objects in the selection set
                    foreach (SelectedObject acSSObj in acSSet)
                    {
                        // Check to make sure a valid SelectedObject object was returned
                        if (acSSObj != null)
                        {
                            // Open the selected object for write
                            Entity acEnt = trans.GetObject(acSSObj.ObjectId, OpenMode.ForWrite) as Entity;
                            if (acEnt != null)
                            {
                                // Change the object's color to Green
                                acEnt.ColorIndex = 3;

                                // Create a matrix and move the circle using a vector
                                ///Select Starting point
                                PromptPointOptions ppo = new PromptPointOptions("Pick starting point: ");
                                PromptPointResult ppr = edt.GetPoint(ppo);
                                Point3d startPt = ppr.Value;

                                ///Select the end point
                                ppo = new PromptPointOptions("Pick end point");
                                ppo.UseBasePoint = true;
                                ppo.BasePoint = startPt;
                                ppr = edt.GetPoint(ppo);
                                Point3d endPt = ppr.Value;

                                ///Creating vector
                                Vector3d destVector = startPt.GetVectorTo(endPt);

                                ///Move object
                                acEnt.TransformBy(Matrix3d.Displacement(destVector));
                            }
                        }
                    }
                    // Commit the transaction
                    trans.Commit();
                }
            }
        }
    }
}
