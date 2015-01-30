using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideBuilder.Test
{
    class OtherTests
    {
        public void foo()
        {
            /*

            Project debugProject = BuilderCore.LoadProject(@"C:\Users\ceztko\Temp\VsProbe\Test\Test.vcxproj",
                collection, globalProperties);
            
            string test = "!$([System.String]::IsNullOrEmpty('$(TargetFrameworkVersion)'))";

            bool test3 = new ExpressionEvaluator(project).EvaluateAsBoolean(test);
            test = "$(TargetFrameworkVersion) == $(TargetFramork) and $(TargetFrameworkVersion) == '4.0'";
            bool success;
            bool? result = new ExpressionEvaluator(project).EvaluateAsBoolean(test, out success);

            ExpressionList exp2 = new ExpressionParser().Parse("Exists('$(MSBuildToolsPath)\\Microsoft.WorkflowBuildExtensions.targets')", ExpressionValidationType.StrictBoolean);
            ExpressionList exp = new ExpressionParser().Parse(test, ExpressionValidationType.StrictBoolean);
             */
        }
    }
}
