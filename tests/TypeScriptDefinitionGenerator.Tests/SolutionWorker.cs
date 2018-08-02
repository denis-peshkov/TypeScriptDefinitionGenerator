using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;

namespace TypeScriptDefinitionGenerator.Tests
{
    public class SolutionWorker
    {
        public ProjectItem GetProjectItem(Solution solution, string filename)
        {
            ProjectItem ret = null;
            // get all the projects
            foreach (Project project in solution.Projects)
            {
                // get all the items in each project
                foreach (ProjectItem item in project.ProjectItems)
                {
                    // find this file and examine it
                    if (item.Name == filename)
                    {
                        ret = item;
                    }
                }
            }

            return ret;
        }

        public void ExamineSolution(Solution solution)
        {
            Console.WriteLine(solution.FullName +" ("+ solution.Projects.Count+")");

            // get all the projects
            foreach (Project project in solution.Projects)
            {
                Console.WriteLine("\t{1}:{2}:{3}:{4}:{5}::::{0}", project.FullName, 
                    project.Kind, 
                    project.CodeModel, 
                    "", 
                    project.Name, 
                    project.ProjectItems.Count);

                // get all the items in each project
                foreach (ProjectItem item in project.ProjectItems)
                {
                    //Console.WriteLine("\t\tProjectItem:{1}: {0}", item.Name, item.ExtenderNames.GetType());
                    Console.WriteLine("\t\tProjectItem: {0}", item.Name);

                    // find this file and examine it "HowToUseCodeModelSpike"
                    if (item.Name == "Constants.cs")
                    {
                        ExamineItem(item);
                    }
                }
            }
        }

        // examine an item
        private void ExamineItem(ProjectItem item)
        {
            FileCodeModel2 model = (FileCodeModel2)item.FileCodeModel;
            foreach (CodeElement codeElement in model.CodeElements)
            {
                ExamineCodeElement(codeElement, 3);
            }
        }

        // recursively examine code elements
        private void ExamineCodeElement(CodeElement codeElement, int tabs)
        {
            tabs++;
            try
            {
                Console.WriteLine(new string('\t', tabs) + "{0} {1}", codeElement.Name, codeElement.Kind.ToString());

                // if this is a namespace, add a class to it.
                if (codeElement.Kind == vsCMElement.vsCMElementNamespace)
                {
                    //AddClassToNamespace((CodeNamespace)codeElement);
                }

                foreach (CodeElement childElement in codeElement.Children)
                {
                    ExamineCodeElement(childElement, tabs);
                }
            }
            catch
            {
                Console.WriteLine(new string('\t', tabs) + "codeElement without name: {0}", codeElement.Kind.ToString());
            }
        }

        // add a class to the given namespace
        private void AddClassToNamespace(CodeNamespace ns)
        {
            // add a class
            CodeClass2 chess = (CodeClass2)ns.AddClass("Chess", -1, null, null, vsCMAccess.vsCMAccessPublic);

            // add a function with a parameter and a comment
            CodeFunction2 move = (CodeFunction2)chess.AddFunction("Move", vsCMFunction.vsCMFunctionFunction, "int", -1, vsCMAccess.vsCMAccessPublic, null);
            move.AddParameter("IsOK", "bool", -1);
            move.Comment = "This is the move function";

            // add some text to the body of the function
            EditPoint2 editPoint = (EditPoint2)move.GetStartPoint(vsCMPart.vsCMPartBody).CreateEditPoint();
            editPoint.Indent(null, 0);
            editPoint.Insert("int a = 1;");
            editPoint.InsertNewLine(1);
            editPoint.Indent(null, 3);
            editPoint.Insert("int b = 3;");
            editPoint.InsertNewLine(2);
            editPoint.Indent(null, 3);
            editPoint.Insert("return a + b; //");
        }

    }
}
