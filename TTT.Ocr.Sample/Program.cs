// See https://aka.ms/new-console-template for more information

using System.Drawing;
using TTT.Ocr;

var engine = Helper.CreateEngine("vi-vn");
var bitmap = (Bitmap) Image.FromFile("zalo_contact_3.png");
var page = await bitmap.Parse(engine);
Console.WriteLine(page.Text);