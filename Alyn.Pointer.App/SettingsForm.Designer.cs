﻿namespace Alyn.Pointer.App
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private System.Windows.Forms.CheckBox soundAlertsCheckBox;
        private System.Windows.Forms.FlowLayoutPanel mainPanel;
        private System.Windows.Forms.Label gazeDelay;
        private System.Windows.Forms.TableLayoutPanel gazePanel;
        private System.Windows.Forms.NumericUpDown gazeDelayUpDown;
    }
}