﻿using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SQL_Format
{
	public partial class FormSQLFormat : Form
	{
		List<Tuple<SplitContainer, SQLTranslator>> items = new List<Tuple<SplitContainer, SQLTranslator>>();

		public FormSQLFormat()
		{
			InitializeComponent();
			TabCtrl.SelectedIndex = 0;

			AddItemByClass(new SQLTranslatorSelect());
			AddItemByClass(new SQLTranslatorUpdate());
			AddItemByClass(new SQLTranslatorMerge());
			AddItemByClass(new SQLTranslatorInsert());
			AddItemByClass(new SQLTranslatorSame());
			AddItemByClass(new SQLTranslatorXmlSelect());
		}

		void AddItemByClass(SQLTranslator t)
		{
			int idx = TabCtrl.TabPages.Count;
			TabCtrl.TabPages.Add(t.GetCaption());
			TabPage tp = TabCtrl.TabPages[idx];

			SplitContainer splitContainer = new SplitContainer();
			tp.Controls.Add(splitContainer);
			TextBox textBox = new TextBox();
			GroupBox groupBox = new GroupBox();

			splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			splitContainer.Location = new System.Drawing.Point(3, 3);
			splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			splitContainer.Panel1.Controls.Add(textBox);
			splitContainer.Panel2.Controls.Add(groupBox);
			splitContainer.Panel2MinSize = 50;
			splitContainer.Size = new System.Drawing.Size(1396, 539);
			splitContainer.SplitterDistance = 510;
			splitContainer.TabIndex = 2;

			textBox.Dock = DockStyle.Fill;
			textBox.Multiline = true;
			textBox.WordWrap = false;
			textBox.Font = MSource.Font;
			textBox.Name = "textbox";
			textBox.ScrollBars = MSource.ScrollBars;

			groupBox.Text = "Options";
			groupBox.Dock = DockStyle.Fill;
			
			FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel();
			flowLayoutPanel.Dock = DockStyle.Fill;
			groupBox.Controls.Add(flowLayoutPanel);

			t.SetupOptionsContent(flowLayoutPanel, OptionChanged);

			items.Add(new Tuple<SplitContainer, SQLTranslator>(splitContainer, t));
		}

		private void OptionChanged(object sender, EventArgs e)
		{
			ParseSource();
		}

		void ParseSource()
		{
			using (StringReader sr = new StringReader(MSource.Text))
			{

				IList<ParseError> errors;
				TSql150Parser parser = new TSql150Parser(true);
				TSqlScript script = parser.Parse(sr, out errors) as TSqlScript;

				CreateTableStatement cts = null;
				foreach (TSqlBatch b in script.Batches)
				{
					foreach(TSqlStatement statement in b.Statements)
					{
						if (!(statement is CreateTableStatement)) continue;

						cts = (CreateTableStatement)statement;

						break;
					}
				}

				foreach (var i in items)
				{
					(i.Item1.Panel1.Controls.Find("textbox", true)[0] as TextBox).Text = "";
					if (cts != null)
					{
						//string s = i.Item2.Translate(cts.Definition, i.Item1);
						string s = i.Item2.TranslateExt(cts, i.Item1);
						(i.Item1.Panel1.Controls.Find("textbox", true)[0] as TextBox).Text = s;
					} else
					{
						if ((errors != null) && (errors.Count > 0))
						{
							(i.Item1.Panel1.Controls.Find("textbox", true)[0] as TextBox).Text = errors[0].Message;
						}
					}
				}

			}
		}

		private void TPSource_Leave(object sender, EventArgs e)
		{
			ParseSource();
		}
	}
}
