﻿/* Copyright (c) Citrix Systems Inc. 
 * All rights reserved. 
 * 
 * Redistribution and use in source and binary forms, 
 * with or without modification, are permitted provided 
 * that the following conditions are met: 
 * 
 * *   Redistributions of source code must retain the above 
 *     copyright notice, this list of conditions and the 
 *     following disclaimer. 
 * *   Redistributions in binary form must reproduce the above 
 *     copyright notice, this list of conditions and the 
 *     following disclaimer in the documentation and/or other 
 *     materials provided with the distribution. 
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND 
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, 
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR 
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, 
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF 
 * SUCH DAMAGE.
 */

using System.Windows.Forms;
using XenAdmin.Commands;
using XenAdmin.Diagnostics.Checks;
using XenAPI;
using XenAdmin.Dialogs.VMDialogs;
using XenAdmin.Actions;
using XenAdmin.Actions.VMActions;
using System.Collections.Generic;
using XenAdmin.Dialogs;
using System.Drawing;
using System;


namespace XenAdmin.Diagnostics.Problems.HostProblem
{
    public class HostOutOfSpaceProblem : HostProblem
    {
        private readonly DiskSpaceRequirements diskSpaceReq;
        private readonly Pool_patch patch;

        public HostOutOfSpaceProblem(Check check, Host host, Pool_patch patch, DiskSpaceRequirements diskSpaceReq)
            : base(check,  host)
        {
            this.patch = patch;
            this.diskSpaceReq = diskSpaceReq;
        }

        public override string Description
        {
            get { return string.Format(Messages.NOT_ENOUGH_SPACE_MESSAGE_INSTALL, Server.Name, patch.Name); }
        }

        protected override AsyncAction CreateAction(out bool cancelled)
        {
            AsyncAction action = null;

            if (diskSpaceReq.CanCleanup)
            {
                Program.Invoke(Program.MainWindow, delegate()
                {
                    DialogResult r = new ThreeButtonDialog(
                        new ThreeButtonDialog.Details(
                           SystemIcons.Warning,
                           diskSpaceReq.GetSpaceRequirementsMessage()),
                        new ThreeButtonDialog.TBDButton(Messages.YES, DialogResult.Yes, ThreeButtonDialog.ButtonType.ACCEPT, true),
                        ThreeButtonDialog.ButtonNo
                        ).ShowDialog(Program.MainWindow);


                    if (r == DialogResult.Yes)
                    {
                        action = new CleanupDiskSpaceAction(this.Server, patch, true);
                    }
                });
            }
            else
            { 
                Program.Invoke(Program.MainWindow, delegate()
                {
                    new ThreeButtonDialog(
                        new ThreeButtonDialog.Details(SystemIcons.Warning, diskSpaceReq.GetSpaceRequirementsMessage()))
                        .ShowDialog();
                });
            }
            cancelled = action == null;
            
            return action;
        }

        public override string HelpMessage
        {
            get { return diskSpaceReq.GetMessageForActionLink(); }
        }

        public override bool IsFixable
        {
            get
            {
                return false;
            }
        }

    }
}
