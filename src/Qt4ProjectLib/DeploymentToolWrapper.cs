/****************************************************************************
**
** Copyright (C) 2012 Digia Plc and/or its subsidiary(-ies).
** Contact: http://www.qt-project.org/legal
**
** This file is part of the Qt VS Add-in.
**
** $QT_BEGIN_LICENSE:LGPL$
** Commercial License Usage
** Licensees holding valid commercial Qt licenses may use this file in
** accordance with the commercial license agreement provided with the
** Software or, alternatively, in accordance with the terms contained in
** a written agreement between you and Digia.  For licensing terms and
** conditions see http://qt.digia.com/licensing.  For further information
** use the contact form at http://qt.digia.com/contact-us.
**
** GNU Lesser General Public License Usage
** Alternatively, this file may be used under the terms of the GNU Lesser
** General Public License version 2.1 as published by the Free Software
** Foundation and appearing in the file LICENSE.LGPL included in the
** packaging of this file.  Please review the following information to
** ensure the GNU Lesser General Public License version 2.1 requirements
** will be met: http://www.gnu.org/licenses/old-licenses/lgpl-2.1.html.
**
** In addition, as a special exception, Digia gives you certain additional
** rights.  These rights are described in the Digia Qt LGPL Exception
** version 1.1, included in the file LGPL_EXCEPTION.txt in this package.
**
** GNU General Public License Usage
** Alternatively, this file may be used under the terms of the GNU
** General Public License version 3.0 as published by the Free Software
** Foundation and appearing in the file LICENSE.GPL included in the
** packaging of this file.  Please review the following information to
** ensure the GNU General Public License version 3.0 requirements will be
** met: http://www.gnu.org/copyleft/gpl.html.
**
**
** $QT_END_LICENSE$
**
****************************************************************************/

using System;
using Microsoft.VisualStudio.VCProjectEngine;

namespace Digia.Qt4ProjectLib
{
    class DeploymentToolWrapper
    {
        object deploymentToolObj;
        Type deploymentToolType;

        public static DeploymentToolWrapper Create(VCConfiguration config)
        {
            DeploymentToolWrapper wrapper = null;
            try
            {
                wrapper = new DeploymentToolWrapper(config.DeploymentTool);
            }
            catch
            {
            }
            return (wrapper.deploymentToolObj == null) ? null : wrapper;
        }

        protected DeploymentToolWrapper(object tool)
        {
            deploymentToolObj = tool;
            deploymentToolType = deploymentToolObj.GetType();
        }

        public void Clear()
        {
            string filesToDeploy = "";
            deploymentToolType.InvokeMember(
                "AdditionalFiles",
                System.Reflection.BindingFlags.SetProperty,
                null,
                deploymentToolObj,
                new object[] { @filesToDeploy });
        }

        public void Add(string filename, string sourceDir, string destDir)
        {
            string filesToDeploy = GetAdditionalFiles();
            if (filesToDeploy.Length > 0)
                filesToDeploy += ";";
            filesToDeploy += filename + "|" + sourceDir + "|" + destDir + "|0";
            SetAdditionalFiles(filesToDeploy);
        }

        public void Remove(string filename, string sourceDir, string destDir)
        {
            string filesToDeploy = GetAdditionalFiles();
            filesToDeploy = filesToDeploy.Replace(filename + "|" + sourceDir + "|" + destDir + "|0", "");
            filesToDeploy = filesToDeploy.Replace(";;", ";");
            if (filesToDeploy.EndsWith(";"))
                filesToDeploy = filesToDeploy.Remove(filesToDeploy.Length - 1);
            SetAdditionalFiles(filesToDeploy);
        }

#if ENABLE_WINCE
        public void AddWinCEMSVCStandardLib(bool isDebugConfiguration, EnvDTE.DTE dte)
        {
            string stdlibname = "msvcr" + dte.Version;
            int idx = stdlibname.IndexOf('.');
            if (idx >= 0) stdlibname = stdlibname.Remove(idx, 1);

            string destDir = RemoteDirectory;
            string dllSuffix = "";
            if (isDebugConfiguration) dllSuffix = "d";
            Add(stdlibname + dllSuffix + ".dll", "$(BINDIR)\\$(INSTRUCTIONSET)", destDir);
        }

        public string RemoteDirectory
        {
            get
            {
                object obj = deploymentToolType.InvokeMember(
                    "RemoteDirectory",
                    System.Reflection.BindingFlags.GetProperty,
                    null,
                    deploymentToolObj,
                    null);

                if (obj != null)
                    return (string)obj;

                return "%CSIDL_PROGRAM_FILES%\\$(ProjectName)";
            }
        }
#endif

        public string GetAdditionalFiles()
        {
            object obj;
            try
            {
                obj = deploymentToolType.InvokeMember(
                    "AdditionalFiles",
                    System.Reflection.BindingFlags.GetProperty,
                    null,
                    deploymentToolObj,
                    null);
            }
            catch
            {
                obj = null;
            }

            if (obj != null)
                return (string)obj;

            return "";
        }

        public void SetAdditionalFiles(string value)
        {
            deploymentToolType.InvokeMember(
                "AdditionalFiles",
                System.Reflection.BindingFlags.SetProperty,
                null,
                deploymentToolObj,
                new object[] { @value });
        }
    }
}
