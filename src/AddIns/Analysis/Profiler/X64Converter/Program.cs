﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.IO;

using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;

namespace X64Converter
{
	class Program
	{
		static int Main(string[] args)
		{
			File.Delete("conversion.log");
			try {
				List<string> map = new List<string>() {
					"..\\Controller\\Profiler",
					"..\\Controller\\Data\\UnmanagedCallTreeNode",
					"..\\Controller\\structs"
				};
				
				foreach (string path in map) {
					CSharpParser parser = new CSharpParser();
					string filePath = path + ".cs";
					
					if (File.Exists(filePath)) {
						using (StreamReader reader = new StreamReader(filePath)) {
							SyntaxTree syntaxTree = parser.Parse(reader, filePath);
							
							if (parser.HasErrors) {
								string message = "Parser errors in file " + filePath + ":\n";
								foreach (Error error in parser.Errors) {
									message += error.Message + "\n";
								}
								Console.WriteLine(message);
								File.WriteAllText(path + "64.cs", message);
								return 2;
							}
							
							syntaxTree.AcceptVisitor(new Converter());
							
							using (StreamWriter writer = new StreamWriter(path + "64.cs")) {
								CSharpOutputVisitor output = new CSharpOutputVisitor(writer, FormattingOptionsFactory.CreateSharpDevelop());
								syntaxTree.AcceptVisitor(output);
							}
						}
					}
				}

				return 0;
			} catch (Exception e) {
				File.WriteAllText("conversion.log", e.ToString());
				return -1;
			}
		}
	}

	class Converter : DepthFirstAstVisitor<object>
	{
		bool copyAllMembers;
		
		public override object VisitSimpleType(SimpleType simpleType)
		{
			simpleType.Identifier = simpleType.Identifier.Replace("32", "64");
			return base.VisitSimpleType(simpleType);
		}

		public override object VisitIdentifierExpression(IdentifierExpression identifierExpression)
		{
			identifierExpression.Identifier = identifierExpression.Identifier.Replace("32", "64");
			return base.VisitIdentifierExpression(identifierExpression);
		}

		public override object VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression)
		{
			memberReferenceExpression.MemberName = memberReferenceExpression.MemberName.Replace("32", "64");
			return base.VisitMemberReferenceExpression(memberReferenceExpression);
		}

		public override object VisitPointerReferenceExpression(PointerReferenceExpression pointerReferenceExpression)
		{
			pointerReferenceExpression.MemberName = pointerReferenceExpression.MemberName.Replace("32", "64");
			return base.VisitPointerReferenceExpression(pointerReferenceExpression);
		}

		public override object VisitMethodDeclaration(MethodDeclaration methodDeclaration)
		{
			if (methodDeclaration.Name.EndsWith("32", StringComparison.Ordinal))
				methodDeclaration.Name = methodDeclaration.Name.Replace("32", "64");
			else {
				if (!this.copyAllMembers)
					methodDeclaration.Remove();
			}
			return base.VisitMethodDeclaration(methodDeclaration);
		}

		public override object VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration)
		{
			if (!this.copyAllMembers)
				propertyDeclaration.Remove();
			return base.VisitPropertyDeclaration(propertyDeclaration);
		}

		public override object VisitFieldDeclaration(FieldDeclaration fieldDeclaration)
		{
			if (!this.copyAllMembers)
				fieldDeclaration.Remove();
			return base.VisitFieldDeclaration(fieldDeclaration);
		}

		public override object VisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration)
		{
			if (!this.copyAllMembers)
				constructorDeclaration.Remove();
			return base.VisitConstructorDeclaration(constructorDeclaration);
		}

		public override object VisitEventDeclaration(EventDeclaration eventDeclaration)
		{
			if (!this.copyAllMembers)
				eventDeclaration.Remove();
			return base.VisitEventDeclaration(eventDeclaration);
		}

		public override object VisitPrimitiveExpression(PrimitiveExpression primitiveExpression)
		{
			if (primitiveExpression.Value is string)
				primitiveExpression.Value = ((string)primitiveExpression.Value).Replace("32", "64");
			return base.VisitPrimitiveExpression(primitiveExpression);
		}

		public override object VisitDestructorDeclaration(DestructorDeclaration destructorDeclaration)
		{
			if (!this.copyAllMembers)
				destructorDeclaration.Remove();
			return base.VisitDestructorDeclaration(destructorDeclaration);
		}

		public override object VisitTypeDeclaration(TypeDeclaration typeDeclaration)
		{
			if (typeDeclaration.Name.EndsWith("32", StringComparison.Ordinal)) {
				this.copyAllMembers = true;
				typeDeclaration.Name = typeDeclaration.Name.Replace("32", "64");
			} else {
				if (!typeDeclaration.Modifiers.HasFlag(Modifiers.Partial))
					typeDeclaration.Remove();
				else
					typeDeclaration.Attributes.Clear();

				this.copyAllMembers = false;
			}
			return base.VisitTypeDeclaration(typeDeclaration);
		}
	}
}
