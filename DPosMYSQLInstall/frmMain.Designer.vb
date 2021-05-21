<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMain))
        Dim DataGridViewCellStyle3 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle4 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.btnAddSchemas = New System.Windows.Forms.Button()
        Me.btnMySQLInstall = New System.Windows.Forms.Button()
        Me.btnAddDposUser = New System.Windows.Forms.Button()
        Me.btnImportMStoMY = New System.Windows.Forms.Button()
        Me.btnNextStep = New System.Windows.Forms.Button()
        Me.grpImportData = New System.Windows.Forms.GroupBox()
        Me.chkRstDPosSys = New System.Windows.Forms.CheckBox()
        Me.chkRstStock = New System.Windows.Forms.CheckBox()
        Me.chkRstTimeClock = New System.Windows.Forms.CheckBox()
        Me.chkRstStreets = New System.Windows.Forms.CheckBox()
        Me.chkRstDeliverit = New System.Windows.Forms.CheckBox()
        Me.btnBrowseBAKFile = New System.Windows.Forms.Button()
        Me.txtBakFile = New System.Windows.Forms.TextBox()
        Me.rbtnImportBakFile = New System.Windows.Forms.RadioButton()
        Me.btnBrowseDumpFile = New System.Windows.Forms.Button()
        Me.txtDumpFile = New System.Windows.Forms.TextBox()
        Me.rbtnImportCloud = New System.Windows.Forms.RadioButton()
        Me.rbtnImportDumpFile = New System.Windows.Forms.RadioButton()
        Me.rbtnImportFile = New System.Windows.Forms.RadioButton()
        Me.btnDropTablesDPosUser = New System.Windows.Forms.Button()
        Me.ProgressBar1 = New System.Windows.Forms.ProgressBar()
        Me.lblProgress = New System.Windows.Forms.Label()
        Me.pnlBasicCtrl = New System.Windows.Forms.Panel()
        Me.lblStep10 = New System.Windows.Forms.Label()
        Me.lblStep9 = New System.Windows.Forms.Label()
        Me.lblStep8 = New System.Windows.Forms.Label()
        Me.lblStep4 = New System.Windows.Forms.Label()
        Me.lblStep7 = New System.Windows.Forms.Label()
        Me.lblStep6 = New System.Windows.Forms.Label()
        Me.lblStep5 = New System.Windows.Forms.Label()
        Me.lblStep3 = New System.Windows.Forms.Label()
        Me.lblStep2 = New System.Windows.Forms.Label()
        Me.pnlAdvanceCtrl = New System.Windows.Forms.Panel()
        Me.txtBackupPath = New System.Windows.Forms.TextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.btnBrowseBackupPath = New System.Windows.Forms.Button()
        Me.btnRestoreCurrent = New System.Windows.Forms.Button()
        Me.btnInstallDPos = New System.Windows.Forms.Button()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.tgSwitchUserType = New MetroFramework.Controls.MetroToggle()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.lblUserTypeStatus = New System.Windows.Forms.Label()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.OpenFileDialog1 = New System.Windows.Forms.OpenFileDialog()
        Me.bwStepByStep = New System.ComponentModel.BackgroundWorker()
        Me.bwMySQLInstall = New System.ComponentModel.BackgroundWorker()
        Me.bwAddDPosUser = New System.ComponentModel.BackgroundWorker()
        Me.bwAddDPosSchemasAndTables = New System.ComponentModel.BackgroundWorker()
        Me.bwImportData = New System.ComponentModel.BackgroundWorker()
        Me.bwDropDposSchemasTables = New System.ComponentModel.BackgroundWorker()
        Me.lblLog = New System.Windows.Forms.Label()
        Me.Column2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridView1 = New System.Windows.Forms.DataGridView()
        Me.lblStepTitle = New System.Windows.Forms.Label()
        Me.lblStepDesc = New System.Windows.Forms.Label()
        Me.grpDPosInstaller = New System.Windows.Forms.GroupBox()
        Me.btnBrowseDPosInstaller = New System.Windows.Forms.Button()
        Me.txtDPosInstaller = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.FolderBrowserDialog1 = New System.Windows.Forms.FolderBrowserDialog()
        Me.grpImportData.SuspendLayout()
        Me.pnlBasicCtrl.SuspendLayout()
        Me.pnlAdvanceCtrl.SuspendLayout()
        Me.Panel2.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpDPosInstaller.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnAddSchemas
        '
        Me.btnAddSchemas.AccessibleRole = System.Windows.Forms.AccessibleRole.None
        Me.btnAddSchemas.BackColor = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(185, Byte), Integer))
        Me.btnAddSchemas.FlatAppearance.BorderSize = 0
        Me.btnAddSchemas.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(CType(CType(32, Byte), Integer), CType(CType(116, Byte), Integer), CType(CType(171, Byte), Integer))
        Me.btnAddSchemas.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(CType(CType(32, Byte), Integer), CType(CType(116, Byte), Integer), CType(CType(171, Byte), Integer))
        Me.btnAddSchemas.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnAddSchemas.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnAddSchemas.ForeColor = System.Drawing.Color.White
        Me.btnAddSchemas.Location = New System.Drawing.Point(11, 100)
        Me.btnAddSchemas.Name = "btnAddSchemas"
        Me.btnAddSchemas.Padding = New System.Windows.Forms.Padding(5, 0, 0, 0)
        Me.btnAddSchemas.Size = New System.Drawing.Size(206, 36)
        Me.btnAddSchemas.TabIndex = 0
        Me.btnAddSchemas.Text = "Generate DPos Schemas and Tables"
        Me.btnAddSchemas.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnAddSchemas.UseVisualStyleBackColor = False
        '
        'btnMySQLInstall
        '
        Me.btnMySQLInstall.AccessibleRole = System.Windows.Forms.AccessibleRole.None
        Me.btnMySQLInstall.BackColor = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(185, Byte), Integer))
        Me.btnMySQLInstall.FlatAppearance.BorderSize = 0
        Me.btnMySQLInstall.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(CType(CType(32, Byte), Integer), CType(CType(116, Byte), Integer), CType(CType(171, Byte), Integer))
        Me.btnMySQLInstall.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(CType(CType(32, Byte), Integer), CType(CType(116, Byte), Integer), CType(CType(171, Byte), Integer))
        Me.btnMySQLInstall.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnMySQLInstall.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnMySQLInstall.ForeColor = System.Drawing.Color.White
        Me.btnMySQLInstall.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnMySQLInstall.Location = New System.Drawing.Point(11, 19)
        Me.btnMySQLInstall.Name = "btnMySQLInstall"
        Me.btnMySQLInstall.Padding = New System.Windows.Forms.Padding(5, 0, 0, 0)
        Me.btnMySQLInstall.Size = New System.Drawing.Size(206, 36)
        Me.btnMySQLInstall.TabIndex = 1
        Me.btnMySQLInstall.Text = "Install MySQL"
        Me.btnMySQLInstall.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnMySQLInstall.UseVisualStyleBackColor = False
        '
        'btnAddDposUser
        '
        Me.btnAddDposUser.AccessibleRole = System.Windows.Forms.AccessibleRole.None
        Me.btnAddDposUser.BackColor = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(185, Byte), Integer))
        Me.btnAddDposUser.FlatAppearance.BorderSize = 0
        Me.btnAddDposUser.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(CType(CType(32, Byte), Integer), CType(CType(116, Byte), Integer), CType(CType(171, Byte), Integer))
        Me.btnAddDposUser.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(CType(CType(32, Byte), Integer), CType(CType(116, Byte), Integer), CType(CType(171, Byte), Integer))
        Me.btnAddDposUser.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnAddDposUser.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnAddDposUser.ForeColor = System.Drawing.Color.White
        Me.btnAddDposUser.Location = New System.Drawing.Point(11, 60)
        Me.btnAddDposUser.Name = "btnAddDposUser"
        Me.btnAddDposUser.Padding = New System.Windows.Forms.Padding(5, 0, 0, 0)
        Me.btnAddDposUser.Size = New System.Drawing.Size(206, 36)
        Me.btnAddDposUser.TabIndex = 2
        Me.btnAddDposUser.Text = "Add DPos User Credentials"
        Me.btnAddDposUser.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnAddDposUser.UseVisualStyleBackColor = False
        '
        'btnImportMStoMY
        '
        Me.btnImportMStoMY.AccessibleRole = System.Windows.Forms.AccessibleRole.None
        Me.btnImportMStoMY.BackColor = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(185, Byte), Integer))
        Me.btnImportMStoMY.FlatAppearance.BorderSize = 0
        Me.btnImportMStoMY.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(CType(CType(32, Byte), Integer), CType(CType(116, Byte), Integer), CType(CType(171, Byte), Integer))
        Me.btnImportMStoMY.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(CType(CType(32, Byte), Integer), CType(CType(116, Byte), Integer), CType(CType(171, Byte), Integer))
        Me.btnImportMStoMY.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnImportMStoMY.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnImportMStoMY.ForeColor = System.Drawing.Color.White
        Me.btnImportMStoMY.Location = New System.Drawing.Point(11, 140)
        Me.btnImportMStoMY.Name = "btnImportMStoMY"
        Me.btnImportMStoMY.Padding = New System.Windows.Forms.Padding(5, 0, 0, 0)
        Me.btnImportMStoMY.Size = New System.Drawing.Size(206, 36)
        Me.btnImportMStoMY.TabIndex = 3
        Me.btnImportMStoMY.Text = "Import Data"
        Me.btnImportMStoMY.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnImportMStoMY.UseVisualStyleBackColor = False
        '
        'btnNextStep
        '
        Me.btnNextStep.BackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.btnNextStep.FlatAppearance.BorderSize = 0
        Me.btnNextStep.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.btnNextStep.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(CType(CType(39, Byte), Integer), CType(CType(139, Byte), Integer), CType(CType(205, Byte), Integer))
        Me.btnNextStep.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnNextStep.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnNextStep.ForeColor = System.Drawing.Color.White
        Me.btnNextStep.Location = New System.Drawing.Point(594, 531)
        Me.btnNextStep.Name = "btnNextStep"
        Me.btnNextStep.Size = New System.Drawing.Size(93, 23)
        Me.btnNextStep.TabIndex = 4
        Me.btnNextStep.Text = "EXECUTE"
        Me.btnNextStep.UseVisualStyleBackColor = False
        '
        'grpImportData
        '
        Me.grpImportData.Controls.Add(Me.chkRstDPosSys)
        Me.grpImportData.Controls.Add(Me.chkRstStock)
        Me.grpImportData.Controls.Add(Me.chkRstTimeClock)
        Me.grpImportData.Controls.Add(Me.chkRstStreets)
        Me.grpImportData.Controls.Add(Me.chkRstDeliverit)
        Me.grpImportData.Controls.Add(Me.btnBrowseBAKFile)
        Me.grpImportData.Controls.Add(Me.txtBakFile)
        Me.grpImportData.Controls.Add(Me.rbtnImportBakFile)
        Me.grpImportData.Controls.Add(Me.btnBrowseDumpFile)
        Me.grpImportData.Controls.Add(Me.txtDumpFile)
        Me.grpImportData.Controls.Add(Me.rbtnImportCloud)
        Me.grpImportData.Controls.Add(Me.rbtnImportDumpFile)
        Me.grpImportData.Controls.Add(Me.rbtnImportFile)
        Me.grpImportData.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpImportData.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.grpImportData.Location = New System.Drawing.Point(234, 139)
        Me.grpImportData.Name = "grpImportData"
        Me.grpImportData.Size = New System.Drawing.Size(453, 200)
        Me.grpImportData.TabIndex = 6
        Me.grpImportData.TabStop = False
        Me.grpImportData.Text = "Import Data Options"
        '
        'chkRstDPosSys
        '
        Me.chkRstDPosSys.AutoSize = True
        Me.chkRstDPosSys.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkRstDPosSys.Location = New System.Drawing.Point(317, 143)
        Me.chkRstDPosSys.Name = "chkRstDPosSys"
        Me.chkRstDPosSys.Size = New System.Drawing.Size(69, 17)
        Me.chkRstDPosSys.TabIndex = 47
        Me.chkRstDPosSys.Text = "DPosSys"
        Me.chkRstDPosSys.UseVisualStyleBackColor = True
        '
        'chkRstStock
        '
        Me.chkRstStock.AutoSize = True
        Me.chkRstStock.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkRstStock.Location = New System.Drawing.Point(257, 143)
        Me.chkRstStock.Name = "chkRstStock"
        Me.chkRstStock.Size = New System.Drawing.Size(54, 17)
        Me.chkRstStock.TabIndex = 46
        Me.chkRstStock.Text = "Stock"
        Me.chkRstStock.UseVisualStyleBackColor = True
        '
        'chkRstTimeClock
        '
        Me.chkRstTimeClock.AutoSize = True
        Me.chkRstTimeClock.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkRstTimeClock.Location = New System.Drawing.Point(175, 143)
        Me.chkRstTimeClock.Name = "chkRstTimeClock"
        Me.chkRstTimeClock.Size = New System.Drawing.Size(76, 17)
        Me.chkRstTimeClock.TabIndex = 45
        Me.chkRstTimeClock.Text = "TimeClock"
        Me.chkRstTimeClock.UseVisualStyleBackColor = True
        '
        'chkRstStreets
        '
        Me.chkRstStreets.AutoSize = True
        Me.chkRstStreets.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkRstStreets.Location = New System.Drawing.Point(110, 143)
        Me.chkRstStreets.Name = "chkRstStreets"
        Me.chkRstStreets.Size = New System.Drawing.Size(59, 17)
        Me.chkRstStreets.TabIndex = 44
        Me.chkRstStreets.Text = "Streets"
        Me.chkRstStreets.UseVisualStyleBackColor = True
        '
        'chkRstDeliverit
        '
        Me.chkRstDeliverit.AutoSize = True
        Me.chkRstDeliverit.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkRstDeliverit.Location = New System.Drawing.Point(35, 143)
        Me.chkRstDeliverit.Name = "chkRstDeliverit"
        Me.chkRstDeliverit.Size = New System.Drawing.Size(69, 17)
        Me.chkRstDeliverit.TabIndex = 43
        Me.chkRstDeliverit.Text = "DeliverIT"
        Me.chkRstDeliverit.UseVisualStyleBackColor = True
        '
        'btnBrowseBAKFile
        '
        Me.btnBrowseBAKFile.Enabled = False
        Me.btnBrowseBAKFile.Location = New System.Drawing.Point(406, 115)
        Me.btnBrowseBAKFile.Name = "btnBrowseBAKFile"
        Me.btnBrowseBAKFile.Size = New System.Drawing.Size(37, 23)
        Me.btnBrowseBAKFile.TabIndex = 7
        Me.btnBrowseBAKFile.Text = "..."
        Me.btnBrowseBAKFile.UseVisualStyleBackColor = True
        '
        'txtBakFile
        '
        Me.txtBakFile.Enabled = False
        Me.txtBakFile.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtBakFile.Location = New System.Drawing.Point(35, 117)
        Me.txtBakFile.Name = "txtBakFile"
        Me.txtBakFile.Size = New System.Drawing.Size(365, 20)
        Me.txtBakFile.TabIndex = 6
        '
        'rbtnImportBakFile
        '
        Me.rbtnImportBakFile.AutoSize = True
        Me.rbtnImportBakFile.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.rbtnImportBakFile.ForeColor = System.Drawing.Color.Black
        Me.rbtnImportBakFile.Location = New System.Drawing.Point(17, 95)
        Me.rbtnImportBakFile.Name = "rbtnImportBakFile"
        Me.rbtnImportBakFile.Size = New System.Drawing.Size(229, 17)
        Me.rbtnImportBakFile.TabIndex = 5
        Me.rbtnImportBakFile.Text = "Import Data from DPos MySQL Backup File"
        Me.rbtnImportBakFile.UseVisualStyleBackColor = True
        '
        'btnBrowseDumpFile
        '
        Me.btnBrowseDumpFile.Enabled = False
        Me.btnBrowseDumpFile.Location = New System.Drawing.Point(406, 67)
        Me.btnBrowseDumpFile.Name = "btnBrowseDumpFile"
        Me.btnBrowseDumpFile.Size = New System.Drawing.Size(37, 23)
        Me.btnBrowseDumpFile.TabIndex = 4
        Me.btnBrowseDumpFile.Text = "..."
        Me.btnBrowseDumpFile.UseVisualStyleBackColor = True
        '
        'txtDumpFile
        '
        Me.txtDumpFile.Enabled = False
        Me.txtDumpFile.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtDumpFile.Location = New System.Drawing.Point(35, 69)
        Me.txtDumpFile.Name = "txtDumpFile"
        Me.txtDumpFile.Size = New System.Drawing.Size(365, 20)
        Me.txtDumpFile.TabIndex = 3
        '
        'rbtnImportCloud
        '
        Me.rbtnImportCloud.AutoSize = True
        Me.rbtnImportCloud.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.rbtnImportCloud.ForeColor = System.Drawing.Color.Black
        Me.rbtnImportCloud.Location = New System.Drawing.Point(17, 172)
        Me.rbtnImportCloud.Name = "rbtnImportCloud"
        Me.rbtnImportCloud.Size = New System.Drawing.Size(133, 17)
        Me.rbtnImportCloud.TabIndex = 2
        Me.rbtnImportCloud.Text = "Import Data from Cloud"
        Me.rbtnImportCloud.UseVisualStyleBackColor = True
        '
        'rbtnImportDumpFile
        '
        Me.rbtnImportDumpFile.AutoSize = True
        Me.rbtnImportDumpFile.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.rbtnImportDumpFile.ForeColor = System.Drawing.Color.Black
        Me.rbtnImportDumpFile.Location = New System.Drawing.Point(17, 47)
        Me.rbtnImportDumpFile.Name = "rbtnImportDumpFile"
        Me.rbtnImportDumpFile.Size = New System.Drawing.Size(191, 17)
        Me.rbtnImportDumpFile.TabIndex = 1
        Me.rbtnImportDumpFile.Text = "Import Data from MySQL Dump File"
        Me.rbtnImportDumpFile.UseVisualStyleBackColor = True
        '
        'rbtnImportFile
        '
        Me.rbtnImportFile.AutoSize = True
        Me.rbtnImportFile.Checked = True
        Me.rbtnImportFile.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.rbtnImportFile.ForeColor = System.Drawing.Color.Black
        Me.rbtnImportFile.Location = New System.Drawing.Point(17, 22)
        Me.rbtnImportFile.Name = "rbtnImportFile"
        Me.rbtnImportFile.Size = New System.Drawing.Size(300, 17)
        Me.rbtnImportFile.TabIndex = 0
        Me.rbtnImportFile.TabStop = True
        Me.rbtnImportFile.Text = "Import Data from local SQL Server using INFILE/OUTFILE"
        Me.rbtnImportFile.UseVisualStyleBackColor = True
        '
        'btnDropTablesDPosUser
        '
        Me.btnDropTablesDPosUser.BackColor = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(185, Byte), Integer))
        Me.btnDropTablesDPosUser.FlatAppearance.BorderSize = 0
        Me.btnDropTablesDPosUser.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(CType(CType(32, Byte), Integer), CType(CType(116, Byte), Integer), CType(CType(171, Byte), Integer))
        Me.btnDropTablesDPosUser.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(CType(CType(32, Byte), Integer), CType(CType(116, Byte), Integer), CType(CType(171, Byte), Integer))
        Me.btnDropTablesDPosUser.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnDropTablesDPosUser.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold)
        Me.btnDropTablesDPosUser.ForeColor = System.Drawing.Color.White
        Me.btnDropTablesDPosUser.Location = New System.Drawing.Point(11, 181)
        Me.btnDropTablesDPosUser.Name = "btnDropTablesDPosUser"
        Me.btnDropTablesDPosUser.Padding = New System.Windows.Forms.Padding(5, 0, 0, 0)
        Me.btnDropTablesDPosUser.Size = New System.Drawing.Size(206, 36)
        Me.btnDropTablesDPosUser.TabIndex = 10
        Me.btnDropTablesDPosUser.Text = "Drop Tables and DPos User"
        Me.btnDropTablesDPosUser.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnDropTablesDPosUser.UseVisualStyleBackColor = False
        '
        'ProgressBar1
        '
        Me.ProgressBar1.Location = New System.Drawing.Point(234, 502)
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.Size = New System.Drawing.Size(453, 23)
        Me.ProgressBar1.TabIndex = 11
        '
        'lblProgress
        '
        Me.lblProgress.AutoSize = True
        Me.lblProgress.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, CType((System.Drawing.FontStyle.Bold Or System.Drawing.FontStyle.Italic), System.Drawing.FontStyle), System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblProgress.Location = New System.Drawing.Point(232, 484)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(64, 15)
        Me.lblProgress.TabIndex = 12
        Me.lblProgress.Text = "Progress"
        '
        'pnlBasicCtrl
        '
        Me.pnlBasicCtrl.BackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.pnlBasicCtrl.Controls.Add(Me.lblStep10)
        Me.pnlBasicCtrl.Controls.Add(Me.lblStep9)
        Me.pnlBasicCtrl.Controls.Add(Me.lblStep8)
        Me.pnlBasicCtrl.Controls.Add(Me.lblStep4)
        Me.pnlBasicCtrl.Controls.Add(Me.lblStep7)
        Me.pnlBasicCtrl.Controls.Add(Me.lblStep6)
        Me.pnlBasicCtrl.Controls.Add(Me.lblStep5)
        Me.pnlBasicCtrl.Controls.Add(Me.lblStep3)
        Me.pnlBasicCtrl.Controls.Add(Me.lblStep2)
        Me.pnlBasicCtrl.Location = New System.Drawing.Point(-2, 81)
        Me.pnlBasicCtrl.Name = "pnlBasicCtrl"
        Me.pnlBasicCtrl.Size = New System.Drawing.Size(228, 481)
        Me.pnlBasicCtrl.TabIndex = 13
        '
        'lblStep10
        '
        Me.lblStep10.BackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.lblStep10.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold)
        Me.lblStep10.ForeColor = System.Drawing.Color.Silver
        Me.lblStep10.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblStep10.Location = New System.Drawing.Point(2, 300)
        Me.lblStep10.Margin = New System.Windows.Forms.Padding(0)
        Me.lblStep10.Name = "lblStep10"
        Me.lblStep10.Padding = New System.Windows.Forms.Padding(5, 0, 0, 0)
        Me.lblStep10.Size = New System.Drawing.Size(226, 36)
        Me.lblStep10.TabIndex = 15
        Me.lblStep10.Text = "     Done"
        Me.lblStep10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblStep9
        '
        Me.lblStep9.BackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.lblStep9.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold)
        Me.lblStep9.ForeColor = System.Drawing.Color.Silver
        Me.lblStep9.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblStep9.Location = New System.Drawing.Point(2, 263)
        Me.lblStep9.Margin = New System.Windows.Forms.Padding(0)
        Me.lblStep9.Name = "lblStep9"
        Me.lblStep9.Padding = New System.Windows.Forms.Padding(5, 0, 0, 0)
        Me.lblStep9.Size = New System.Drawing.Size(226, 36)
        Me.lblStep9.TabIndex = 14
        Me.lblStep9.Text = "     Open Cloud Sync App"
        Me.lblStep9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblStep8
        '
        Me.lblStep8.BackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.lblStep8.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold)
        Me.lblStep8.ForeColor = System.Drawing.Color.Silver
        Me.lblStep8.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblStep8.Location = New System.Drawing.Point(2, 226)
        Me.lblStep8.Margin = New System.Windows.Forms.Padding(0)
        Me.lblStep8.Name = "lblStep8"
        Me.lblStep8.Padding = New System.Windows.Forms.Padding(5, 0, 0, 0)
        Me.lblStep8.Size = New System.Drawing.Size(226, 36)
        Me.lblStep8.TabIndex = 12
        Me.lblStep8.Text = "     Installing DPos"
        Me.lblStep8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblStep4
        '
        Me.lblStep4.BackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.lblStep4.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold)
        Me.lblStep4.ForeColor = System.Drawing.Color.Silver
        Me.lblStep4.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblStep4.Location = New System.Drawing.Point(2, 82)
        Me.lblStep4.Margin = New System.Windows.Forms.Padding(0)
        Me.lblStep4.Name = "lblStep4"
        Me.lblStep4.Padding = New System.Windows.Forms.Padding(5, 0, 0, 0)
        Me.lblStep4.Size = New System.Drawing.Size(226, 36)
        Me.lblStep4.TabIndex = 11
        Me.lblStep4.Text = "     Setting Up Import Options"
        Me.lblStep4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblStep7
        '
        Me.lblStep7.BackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.lblStep7.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold)
        Me.lblStep7.ForeColor = System.Drawing.Color.Silver
        Me.lblStep7.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblStep7.Location = New System.Drawing.Point(2, 190)
        Me.lblStep7.Margin = New System.Windows.Forms.Padding(0)
        Me.lblStep7.Name = "lblStep7"
        Me.lblStep7.Padding = New System.Windows.Forms.Padding(5, 0, 0, 0)
        Me.lblStep7.Size = New System.Drawing.Size(226, 36)
        Me.lblStep7.TabIndex = 10
        Me.lblStep7.Text = "     Locating DPos Installer"
        Me.lblStep7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblStep6
        '
        Me.lblStep6.BackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.lblStep6.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold)
        Me.lblStep6.ForeColor = System.Drawing.Color.Silver
        Me.lblStep6.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblStep6.Location = New System.Drawing.Point(2, 154)
        Me.lblStep6.Margin = New System.Windows.Forms.Padding(0)
        Me.lblStep6.Name = "lblStep6"
        Me.lblStep6.Padding = New System.Windows.Forms.Padding(5, 0, 0, 0)
        Me.lblStep6.Size = New System.Drawing.Size(226, 36)
        Me.lblStep6.TabIndex = 9
        Me.lblStep6.Text = "     Import Data"
        Me.lblStep6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblStep5
        '
        Me.lblStep5.BackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.lblStep5.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold)
        Me.lblStep5.ForeColor = System.Drawing.Color.Silver
        Me.lblStep5.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblStep5.Location = New System.Drawing.Point(2, 118)
        Me.lblStep5.Margin = New System.Windows.Forms.Padding(0)
        Me.lblStep5.Name = "lblStep5"
        Me.lblStep5.Padding = New System.Windows.Forms.Padding(5, 0, 0, 0)
        Me.lblStep5.Size = New System.Drawing.Size(226, 36)
        Me.lblStep5.TabIndex = 8
        Me.lblStep5.Text = "     Generate DPos Schemas and Tables"
        Me.lblStep5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblStep3
        '
        Me.lblStep3.BackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.lblStep3.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold)
        Me.lblStep3.ForeColor = System.Drawing.Color.Silver
        Me.lblStep3.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblStep3.Location = New System.Drawing.Point(2, 46)
        Me.lblStep3.Margin = New System.Windows.Forms.Padding(0)
        Me.lblStep3.Name = "lblStep3"
        Me.lblStep3.Padding = New System.Windows.Forms.Padding(5, 0, 0, 0)
        Me.lblStep3.Size = New System.Drawing.Size(226, 36)
        Me.lblStep3.TabIndex = 7
        Me.lblStep3.Text = "     Add DPos User Credentials"
        Me.lblStep3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblStep2
        '
        Me.lblStep2.BackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.lblStep2.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold)
        Me.lblStep2.ForeColor = System.Drawing.Color.Silver
        Me.lblStep2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblStep2.Location = New System.Drawing.Point(2, 10)
        Me.lblStep2.Margin = New System.Windows.Forms.Padding(0)
        Me.lblStep2.Name = "lblStep2"
        Me.lblStep2.Padding = New System.Windows.Forms.Padding(5, 0, 0, 0)
        Me.lblStep2.Size = New System.Drawing.Size(226, 36)
        Me.lblStep2.TabIndex = 6
        Me.lblStep2.Text = "     Install MySQL"
        Me.lblStep2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'pnlAdvanceCtrl
        '
        Me.pnlAdvanceCtrl.BackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.pnlAdvanceCtrl.Controls.Add(Me.txtBackupPath)
        Me.pnlAdvanceCtrl.Controls.Add(Me.Label4)
        Me.pnlAdvanceCtrl.Controls.Add(Me.Label3)
        Me.pnlAdvanceCtrl.Controls.Add(Me.btnBrowseBackupPath)
        Me.pnlAdvanceCtrl.Controls.Add(Me.btnRestoreCurrent)
        Me.pnlAdvanceCtrl.Controls.Add(Me.btnInstallDPos)
        Me.pnlAdvanceCtrl.Controls.Add(Me.btnMySQLInstall)
        Me.pnlAdvanceCtrl.Controls.Add(Me.btnAddSchemas)
        Me.pnlAdvanceCtrl.Controls.Add(Me.btnAddDposUser)
        Me.pnlAdvanceCtrl.Controls.Add(Me.btnImportMStoMY)
        Me.pnlAdvanceCtrl.Controls.Add(Me.btnDropTablesDPosUser)
        Me.pnlAdvanceCtrl.Location = New System.Drawing.Point(-2, 81)
        Me.pnlAdvanceCtrl.Name = "pnlAdvanceCtrl"
        Me.pnlAdvanceCtrl.Size = New System.Drawing.Size(228, 481)
        Me.pnlAdvanceCtrl.TabIndex = 15
        '
        'txtBackupPath
        '
        Me.txtBackupPath.Enabled = False
        Me.txtBackupPath.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtBackupPath.Location = New System.Drawing.Point(8, 388)
        Me.txtBackupPath.Name = "txtBackupPath"
        Me.txtBackupPath.Size = New System.Drawing.Size(171, 20)
        Me.txtBackupPath.TabIndex = 17
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label4.ForeColor = System.Drawing.Color.White
        Me.Label4.Location = New System.Drawing.Point(9, 410)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(217, 13)
        Me.Label4.TabIndex = 20
        Me.Label4.Text = "Leave it blank to use the default location"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold)
        Me.Label3.ForeColor = System.Drawing.Color.White
        Me.Label3.Location = New System.Drawing.Point(7, 372)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(117, 13)
        Me.Label3.TabIndex = 19
        Me.Label3.Text = "Create backup file to:"
        '
        'btnBrowseBackupPath
        '
        Me.btnBrowseBackupPath.Enabled = False
        Me.btnBrowseBackupPath.Location = New System.Drawing.Point(183, 386)
        Me.btnBrowseBackupPath.Name = "btnBrowseBackupPath"
        Me.btnBrowseBackupPath.Size = New System.Drawing.Size(37, 23)
        Me.btnBrowseBackupPath.TabIndex = 18
        Me.btnBrowseBackupPath.Text = "..."
        Me.btnBrowseBackupPath.UseVisualStyleBackColor = True
        '
        'btnRestoreCurrent
        '
        Me.btnRestoreCurrent.BackColor = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(185, Byte), Integer))
        Me.btnRestoreCurrent.FlatAppearance.BorderSize = 0
        Me.btnRestoreCurrent.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(CType(CType(32, Byte), Integer), CType(CType(116, Byte), Integer), CType(CType(171, Byte), Integer))
        Me.btnRestoreCurrent.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(CType(CType(32, Byte), Integer), CType(CType(116, Byte), Integer), CType(CType(171, Byte), Integer))
        Me.btnRestoreCurrent.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnRestoreCurrent.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold)
        Me.btnRestoreCurrent.ForeColor = System.Drawing.Color.White
        Me.btnRestoreCurrent.Location = New System.Drawing.Point(11, 429)
        Me.btnRestoreCurrent.Name = "btnRestoreCurrent"
        Me.btnRestoreCurrent.Padding = New System.Windows.Forms.Padding(5, 0, 0, 0)
        Me.btnRestoreCurrent.Size = New System.Drawing.Size(206, 36)
        Me.btnRestoreCurrent.TabIndex = 12
        Me.btnRestoreCurrent.Text = "Backup Current MySQL Data"
        Me.btnRestoreCurrent.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnRestoreCurrent.UseVisualStyleBackColor = False
        '
        'btnInstallDPos
        '
        Me.btnInstallDPos.BackColor = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(185, Byte), Integer))
        Me.btnInstallDPos.FlatAppearance.BorderSize = 0
        Me.btnInstallDPos.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(CType(CType(32, Byte), Integer), CType(CType(116, Byte), Integer), CType(CType(171, Byte), Integer))
        Me.btnInstallDPos.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(CType(CType(32, Byte), Integer), CType(CType(116, Byte), Integer), CType(CType(171, Byte), Integer))
        Me.btnInstallDPos.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnInstallDPos.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold)
        Me.btnInstallDPos.ForeColor = System.Drawing.Color.White
        Me.btnInstallDPos.Location = New System.Drawing.Point(11, 222)
        Me.btnInstallDPos.Name = "btnInstallDPos"
        Me.btnInstallDPos.Padding = New System.Windows.Forms.Padding(5, 0, 0, 0)
        Me.btnInstallDPos.Size = New System.Drawing.Size(206, 36)
        Me.btnInstallDPos.TabIndex = 11
        Me.btnInstallDPos.Text = "Install DPos"
        Me.btnInstallDPos.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnInstallDPos.UseVisualStyleBackColor = False
        '
        'Panel2
        '
        Me.Panel2.BackColor = System.Drawing.Color.White
        Me.Panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Panel2.Controls.Add(Me.Button1)
        Me.Panel2.Controls.Add(Me.tgSwitchUserType)
        Me.Panel2.Controls.Add(Me.Label2)
        Me.Panel2.Controls.Add(Me.lblUserTypeStatus)
        Me.Panel2.Controls.Add(Me.PictureBox1)
        Me.Panel2.Location = New System.Drawing.Point(-1, -1)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(697, 85)
        Me.Panel2.TabIndex = 14
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(606, 12)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(75, 23)
        Me.Button1.TabIndex = 5
        Me.Button1.Text = "Button1"
        Me.Button1.UseVisualStyleBackColor = True
        Me.Button1.Visible = False
        '
        'tgSwitchUserType
        '
        Me.tgSwitchUserType.AutoSize = True
        Me.tgSwitchUserType.DisplayStatus = False
        Me.tgSwitchUserType.Location = New System.Drawing.Point(637, 61)
        Me.tgSwitchUserType.Name = "tgSwitchUserType"
        Me.tgSwitchUserType.Size = New System.Drawing.Size(50, 17)
        Me.tgSwitchUserType.TabIndex = 3
        Me.tgSwitchUserType.Text = "Off"
        Me.tgSwitchUserType.Theme = MetroFramework.MetroThemeStyle.Light
        Me.tgSwitchUserType.UseStyleColors = True
        Me.tgSwitchUserType.UseVisualStyleBackColor = True
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Arial", 14.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.Label2.Location = New System.Drawing.Point(159, 35)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(288, 22)
        Me.Label2.TabIndex = 1
        Me.Label2.Text = "MySQL and Data Import Client"
        '
        'lblUserTypeStatus
        '
        Me.lblUserTypeStatus.AutoSize = True
        Me.lblUserTypeStatus.Font = New System.Drawing.Font("Segoe UI Semilight", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblUserTypeStatus.Location = New System.Drawing.Point(483, 63)
        Me.lblUserTypeStatus.Name = "lblUserTypeStatus"
        Me.lblUserTypeStatus.Size = New System.Drawing.Size(151, 13)
        Me.lblUserTypeStatus.TabIndex = 4
        Me.lblUserTypeStatus.Text = "Switch to Advanced Installation"
        '
        'PictureBox1
        '
        Me.PictureBox1.Image = CType(resources.GetObject("PictureBox1.Image"), System.Drawing.Image)
        Me.PictureBox1.Location = New System.Drawing.Point(5, 4)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(146, 74)
        Me.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
        Me.PictureBox1.TabIndex = 0
        Me.PictureBox1.TabStop = False
        '
        'OpenFileDialog1
        '
        Me.OpenFileDialog1.FileName = "OpenFileDialog1"
        '
        'bwStepByStep
        '
        Me.bwStepByStep.WorkerReportsProgress = True
        '
        'bwMySQLInstall
        '
        Me.bwMySQLInstall.WorkerReportsProgress = True
        '
        'bwAddDPosUser
        '
        Me.bwAddDPosUser.WorkerReportsProgress = True
        '
        'bwAddDPosSchemasAndTables
        '
        Me.bwAddDPosSchemasAndTables.WorkerReportsProgress = True
        '
        'bwImportData
        '
        Me.bwImportData.WorkerReportsProgress = True
        '
        'bwDropDposSchemasTables
        '
        Me.bwDropDposSchemasTables.WorkerReportsProgress = True
        '
        'lblLog
        '
        Me.lblLog.AutoSize = True
        Me.lblLog.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblLog.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.lblLog.Location = New System.Drawing.Point(235, 205)
        Me.lblLog.Margin = New System.Windows.Forms.Padding(0)
        Me.lblLog.Name = "lblLog"
        Me.lblLog.Size = New System.Drawing.Size(32, 13)
        Me.lblLog.TabIndex = 7
        Me.lblLog.Text = "Log:"
        '
        'Column2
        '
        Me.Column2.HeaderText = "Column2"
        Me.Column2.Name = "Column2"
        Me.Column2.ReadOnly = True
        '
        'Column1
        '
        Me.Column1.HeaderText = "Column1"
        Me.Column1.Name = "Column1"
        Me.Column1.ReadOnly = True
        '
        'DataGridView1
        '
        Me.DataGridView1.AccessibleRole = System.Windows.Forms.AccessibleRole.SplitButton
        Me.DataGridView1.AllowUserToDeleteRows = False
        Me.DataGridView1.AllowUserToResizeColumns = False
        Me.DataGridView1.AllowUserToResizeRows = False
        Me.DataGridView1.BackgroundColor = System.Drawing.Color.White
        Me.DataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.DataGridView1.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None
        Me.DataGridView1.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable
        Me.DataGridView1.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None
        Me.DataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView1.ColumnHeadersVisible = False
        Me.DataGridView1.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Column1, Me.Column2})
        DataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft
        DataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window
        DataGridViewCellStyle3.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.White
        DataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.Black
        DataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.DataGridView1.DefaultCellStyle = DataGridViewCellStyle3
        Me.DataGridView1.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically
        Me.DataGridView1.Location = New System.Drawing.Point(234, 344)
        Me.DataGridView1.Margin = New System.Windows.Forms.Padding(0)
        Me.DataGridView1.Name = "DataGridView1"
        Me.DataGridView1.ReadOnly = True
        Me.DataGridView1.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None
        Me.DataGridView1.RowHeadersVisible = False
        Me.DataGridView1.RowHeadersWidth = 14
        DataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft
        DataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.DataGridView1.RowsDefaultCellStyle = DataGridViewCellStyle4
        Me.DataGridView1.RowTemplate.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft
        Me.DataGridView1.RowTemplate.Height = 14
        Me.DataGridView1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.DataGridView1.Size = New System.Drawing.Size(453, 249)
        Me.DataGridView1.TabIndex = 9
        '
        'lblStepTitle
        '
        Me.lblStepTitle.AutoSize = True
        Me.lblStepTitle.Font = New System.Drawing.Font("Segoe UI", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblStepTitle.ForeColor = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(185, Byte), Integer))
        Me.lblStepTitle.Location = New System.Drawing.Point(237, 91)
        Me.lblStepTitle.Name = "lblStepTitle"
        Me.lblStepTitle.Size = New System.Drawing.Size(295, 20)
        Me.lblStepTitle.TabIndex = 16
        Me.lblStepTitle.Text = "Welcome to MySQL and Data Import Client"
        '
        'lblStepDesc
        '
        Me.lblStepDesc.AutoSize = True
        Me.lblStepDesc.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblStepDesc.Location = New System.Drawing.Point(239, 117)
        Me.lblStepDesc.Name = "lblStepDesc"
        Me.lblStepDesc.Size = New System.Drawing.Size(278, 13)
        Me.lblStepDesc.TabIndex = 17
        Me.lblStepDesc.Text = "This app will guide you though the steps required to install"
        '
        'grpDPosInstaller
        '
        Me.grpDPosInstaller.Controls.Add(Me.btnBrowseDPosInstaller)
        Me.grpDPosInstaller.Controls.Add(Me.txtDPosInstaller)
        Me.grpDPosInstaller.Controls.Add(Me.Label1)
        Me.grpDPosInstaller.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold)
        Me.grpDPosInstaller.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.grpDPosInstaller.Location = New System.Drawing.Point(242, 372)
        Me.grpDPosInstaller.Name = "grpDPosInstaller"
        Me.grpDPosInstaller.Size = New System.Drawing.Size(452, 76)
        Me.grpDPosInstaller.TabIndex = 18
        Me.grpDPosInstaller.TabStop = False
        Me.grpDPosInstaller.Text = "DPos Installer Location"
        '
        'btnBrowseDPosInstaller
        '
        Me.btnBrowseDPosInstaller.Enabled = False
        Me.btnBrowseDPosInstaller.Location = New System.Drawing.Point(410, 30)
        Me.btnBrowseDPosInstaller.Name = "btnBrowseDPosInstaller"
        Me.btnBrowseDPosInstaller.Size = New System.Drawing.Size(37, 23)
        Me.btnBrowseDPosInstaller.TabIndex = 6
        Me.btnBrowseDPosInstaller.Text = "..."
        Me.btnBrowseDPosInstaller.UseVisualStyleBackColor = True
        '
        'txtDPosInstaller
        '
        Me.txtDPosInstaller.Enabled = False
        Me.txtDPosInstaller.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtDPosInstaller.Location = New System.Drawing.Point(58, 32)
        Me.txtDPosInstaller.Name = "txtDPosInstaller"
        Me.txtDPosInstaller.Size = New System.Drawing.Size(346, 20)
        Me.txtDPosInstaller.TabIndex = 5
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!)
        Me.Label1.ForeColor = System.Drawing.Color.Black
        Me.Label1.Location = New System.Drawing.Point(10, 35)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(42, 13)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Browse"
        '
        'frmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.SystemColors.Control
        Me.ClientSize = New System.Drawing.Size(695, 558)
        Me.Controls.Add(Me.grpDPosInstaller)
        Me.Controls.Add(Me.lblStepDesc)
        Me.Controls.Add(Me.lblStepTitle)
        Me.Controls.Add(Me.DataGridView1)
        Me.Controls.Add(Me.Panel2)
        Me.Controls.Add(Me.lblLog)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.btnNextStep)
        Me.Controls.Add(Me.ProgressBar1)
        Me.Controls.Add(Me.grpImportData)
        Me.Controls.Add(Me.pnlBasicCtrl)
        Me.Controls.Add(Me.pnlAdvanceCtrl)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximumSize = New System.Drawing.Size(711, 597)
        Me.Name = "frmMain"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "DPos MySQL Install"
        Me.grpImportData.ResumeLayout(False)
        Me.grpImportData.PerformLayout()
        Me.pnlBasicCtrl.ResumeLayout(False)
        Me.pnlAdvanceCtrl.ResumeLayout(False)
        Me.pnlAdvanceCtrl.PerformLayout()
        Me.Panel2.ResumeLayout(False)
        Me.Panel2.PerformLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpDPosInstaller.ResumeLayout(False)
        Me.grpDPosInstaller.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents btnAddSchemas As Button
    Friend WithEvents btnMySQLInstall As Button
    Friend WithEvents btnAddDposUser As Button
    Friend WithEvents btnImportMStoMY As Button
    Friend WithEvents btnNextStep As Button
    Friend WithEvents grpImportData As GroupBox
    Friend WithEvents rbtnImportCloud As RadioButton
    Friend WithEvents rbtnImportDumpFile As RadioButton
    Friend WithEvents rbtnImportFile As RadioButton
    Friend WithEvents txtDumpFile As TextBox
    Friend WithEvents btnBrowseDumpFile As Button
    Friend WithEvents btnDropTablesDPosUser As Button
    Friend WithEvents ProgressBar1 As ProgressBar
    Friend WithEvents lblProgress As Label
    Friend WithEvents pnlBasicCtrl As Panel
    Friend WithEvents Panel2 As Panel
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents Label2 As Label
    Friend WithEvents lblStep6 As Label
    Friend WithEvents lblStep5 As Label
    Friend WithEvents lblStep3 As Label
    Friend WithEvents lblStep2 As Label
    Friend WithEvents lblStep7 As Label
    Friend WithEvents pnlAdvanceCtrl As Panel
    Friend WithEvents lblUserTypeStatus As Label
    Friend WithEvents tgSwitchUserType As MetroFramework.Controls.MetroToggle
    Friend WithEvents btnBrowseBAKFile As Button
    Friend WithEvents txtBakFile As TextBox
    Friend WithEvents rbtnImportBakFile As RadioButton
    Friend WithEvents OpenFileDialog1 As OpenFileDialog
    Friend WithEvents chkRstDPosSys As CheckBox
    Friend WithEvents chkRstStock As CheckBox
    Friend WithEvents chkRstTimeClock As CheckBox
    Friend WithEvents chkRstStreets As CheckBox
    Friend WithEvents chkRstDeliverit As CheckBox
    Friend WithEvents bwMySQLInstall As System.ComponentModel.BackgroundWorker
    Friend WithEvents bwStepByStep As System.ComponentModel.BackgroundWorker
    Friend WithEvents bwAddDPosUser As System.ComponentModel.BackgroundWorker
    Friend WithEvents bwAddDPosSchemasAndTables As System.ComponentModel.BackgroundWorker
    Friend WithEvents bwImportData As System.ComponentModel.BackgroundWorker
    Friend WithEvents bwDropDposSchemasTables As System.ComponentModel.BackgroundWorker
    Friend WithEvents lblLog As Label
    Friend WithEvents Column2 As DataGridViewTextBoxColumn
    Friend WithEvents Column1 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridView1 As DataGridView
    Friend WithEvents lblStep4 As Label
    Friend WithEvents lblStepTitle As Label
    Friend WithEvents lblStepDesc As Label
    Friend WithEvents lblStep8 As Label
    Friend WithEvents grpDPosInstaller As GroupBox
    Friend WithEvents btnBrowseDPosInstaller As Button
    Friend WithEvents txtDPosInstaller As TextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents btnInstallDPos As Button
    Friend WithEvents lblStep9 As Label
    Friend WithEvents Button1 As Button
    Friend WithEvents lblStep10 As Label
    Friend WithEvents btnRestoreCurrent As Button
    Friend WithEvents FolderBrowserDialog1 As FolderBrowserDialog
    Friend WithEvents txtBackupPath As TextBox
    Friend WithEvents Label4 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents btnBrowseBackupPath As Button
End Class
