/*
 * SubSonic - http://subsonicproject.com
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an 
 * "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
*/

using System.ServiceProcess;
using System.Transactions;
using MbUnit.Framework;
using Northwind;
using SubSonic;
using SubSonic.Parser;
using System.Data;
using System.Xml;
using SubSonic.WebUtility;
using MbUnit.Framework.Reflection;
using System.Web;

namespace SubSonic.Tests
{
    public class RESTHandlerTests
    {
        RESTHandler handler;
        Reflector reflect;
        RESTfullUrl currentUrl;

        [SetUp]
        public void Setup()
        {
            handler = new RESTHandler();
            reflect = new Reflector(handler);
            currentUrl = new RESTfullUrl("http://foo.com/services/data/product/list.json");
            reflect.SetField("_url", currentUrl);
        }

        [TearDown]
        public void TearDown()
        {
            handler = null;
            reflect = null;
            currentUrl = null;
        }

        [Test]
        public void TestXMLResultStringSingleRecord()
        {
            Query q = new Query(Product.Schema).WHERE(Product.Columns.ProductID, 1);
            DataSet ds = q.ExecuteDataSet();
            string result = handler.FormatOutput(ds);
            string expectedResult = "<Products>\r\n  <Product>\r\n    <ProductID>1</ProductID>\r\n    <ProductName>Chai</ProductName>\r\n    <SupplierID>16</SupplierID>\r\n    <CategoryID>1</CategoryID>\r\n    <QuantityPerUnit>10 boxes x 20 bags</QuantityPerUnit>\r\n    <UnitPrice>50.0000</UnitPrice>\r\n    <UnitsInStock>39</UnitsInStock>\r\n    <UnitsOnOrder>0</UnitsOnOrder>\r\n    <ReorderLevel>10</ReorderLevel>\r\n    <Discontinued>false</Discontinued>\r\n    <AttributeXML>&lt;ArrayOfProductAttribute xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"&gt;&lt;ProductAttribute&gt;&lt;AttributeName&gt;Test1&lt;/AttributeName&gt;&lt;AttributeValue xsi:type=\"xsd:string\"&gt;String Value&lt;/AttributeValue&gt;&lt;CurrencyOffset&gt;12&lt;/CurrencyOffset&gt;&lt;/ProductAttribute&gt;&lt;ProductAttribute&gt;&lt;AttributeName&gt;Test2&lt;/AttributeName&gt;&lt;AttributeValue xsi:type=\"xsd:dateTime\"&gt;2007-03-09T10:42:00.6517547-10:00&lt;/AttributeValue&gt;&lt;CurrencyOffset&gt;-100&lt;/CurrencyOffset&gt;&lt;/ProductAttribute&gt;&lt;/ArrayOfProductAttribute&gt;</AttributeXML>\r\n    <DateCreated>2007-05-22T19:44:42-06:00</DateCreated>\r\n    <ProductGUID>52c7760f-6b66-46e7-a99b-67cf142f2a6c</ProductGUID>\r\n    <CreatedOn>2007-05-22T19:43:04-06:00</CreatedOn>\r\n    <ModifiedOn>2007-05-24T06:31:51-06:00</ModifiedOn>\r\n    <ModifiedBy>Unit Test</ModifiedBy>\r\n    <Deleted>false</Deleted>\r\n  </Product>\r\n</Products>";
            Assert.AreEqual(expectedResult, result);
        }
        
        [Test]
        public void TestXMLResultStringZeroRecords()
        {
            Query q = new Query(Product.Schema).WHERE(Product.Columns.ProductID, int.MaxValue);
            DataSet ds = q.ExecuteDataSet();
            string result = handler.FormatOutput(ds);
            string expectedResult = "<Products><Product/></Products>";
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void RESTfullUrlReturnTypeIsSetToJSON()
        {
            string url = "http://foo.com/services/data/product/list.json";
            RESTfullUrl me = new RESTfullUrl(url);
            Assert.AreEqual(url, (string)(new Reflector(me).GetField("_rawUrl")));
            Assert.AreEqual(RESTReturnType.json, me.ReturnType);
        }

        [Test]
        public void RESTfullUrlReturnTypeIsSetToXML()
        {
            string url = "http://foo.com/services/data/product/list.xml";
            RESTfullUrl me = new RESTfullUrl(url);
            Assert.AreEqual(url, (string)(new Reflector(me).GetField("_rawUrl")));
            Assert.AreEqual(RESTReturnType.xml, me.ReturnType);
        }

        [Test]
        [DependsOn("RESTfullUrlReturnTypeIsSetToJSON")]
        [DependsOn("RESTfullUrlReturnTypeIsSetToXML")]
        public void TestJSONResultStringZeroRecords()
        {
            Query q = new Query(Product.Schema).WHERE(Product.Columns.ProductID, int.MaxValue);
            DataSet ds = q.ExecuteDataSet();
            string result = handler.FormatOutput(ds);
            RESTfullContent content = handler.ConvertOutputToEndFormat(result, currentUrl);
            Assert.AreEqual("text/json", content.ContentType);
            string expectedResult = "{ \"Products\": {\"Product\": null }}";
            Assert.AreEqual(expectedResult, content.Content);
        }

        [Test]
        [DependsOn("TestJSONResultStringZeroRecords")]
        public void TestJSONResultStringSingleRecord()
        {
            Query q = new Query(Product.Schema).WHERE(Product.Columns.ProductID, 1);
            DataSet ds = q.ExecuteDataSet();
            string result = handler.FormatOutput(ds);
            RESTfullContent content = handler.ConvertOutputToEndFormat(result, currentUrl);
            Assert.AreEqual("text/json", content.ContentType);
            string expectedResult = "{ \"Products\": {\"Product\": {\"AttributeXML\": \"<ArrayOfProductAttribute xmlns:xsi=\\\\\"http://www.w3.org/2001/XMLSchema-instance\\\\\" xmlns:xsd=\\\\\"http://www.w3.org/2001/XMLSchema\\\\\"\\\\><ProductAttribute\\\\><AttributeName\\\\>Test1</AttributeName\\\\><AttributeValue xsi:type=\\\\\"xsd:string\\\\\"\\\\>String Value</AttributeValue\\\\><CurrencyOffset\\\\>12</CurrencyOffset\\\\></ProductAttribute\\\\><ProductAttribute\\\\><AttributeName\\\\>Test2</AttributeName\\\\><AttributeValue xsi:type=\\\\\"xsd:dateTime\\\\\"\\\\>2007-03-09T10:42:00.6517547-10:00</AttributeValue\\\\><CurrencyOffset\\\\>-100</CurrencyOffset\\\\></ProductAttribute\\\\></ArrayOfProductAttribute\\\\>\", \"CategoryID\": \"1\", \"CreatedOn\": \"2007-05-22T19:43:04-06:00\", \"DateCreated\": \"2007-05-22T19:44:42-06:00\", \"Deleted\": \"false\", \"Discontinued\": \"false\", \"ModifiedBy\": \"Unit Test\", \"ModifiedOn\": \"2007-05-24T06:31:51-06:00\", \"ProductGUID\": \"52c7760f-6b66-46e7-a99b-67cf142f2a6c\", \"ProductID\": \"1\", \"ProductName\": \"Chai\", \"QuantityPerUnit\": \"10 boxes x 20 bags\", \"ReorderLevel\": \"10\", \"SupplierID\": \"16\", \"UnitPrice\": \"50.0000\", \"UnitsInStock\": \"39\", \"UnitsOnOrder\": \"0\" } }}";
            Assert.AreEqual(expectedResult, content.Content);
        }

        [Test]
        [DependsOn("TestJSONResultStringZeroRecords")]
        public void TestJSONResultStringMultipleRecords()
        {
            Query q = new Query(Product.Schema).WHERE(Product.Columns.ProductID, SubSonic.Comparison.In, new int[] {1,2});
            DataSet ds = q.ExecuteDataSet();
            string result = handler.FormatOutput(ds);
            RESTfullContent content = handler.ConvertOutputToEndFormat(result, currentUrl);
            Assert.AreEqual("text/json", content.ContentType);
            string expectedResult = "{ \"Products\": {\"Product\": [ {\"AttributeXML\": \"<ArrayOfProductAttribute xmlns:xsi=\\\\\"http://www.w3.org/2001/XMLSchema-instance\\\\\" xmlns:xsd=\\\\\"http://www.w3.org/2001/XMLSchema\\\\\"\\\\><ProductAttribute\\\\><AttributeName\\\\>Test1</AttributeName\\\\><AttributeValue xsi:type=\\\\\"xsd:string\\\\\"\\\\>String Value</AttributeValue\\\\><CurrencyOffset\\\\>12</CurrencyOffset\\\\></ProductAttribute\\\\><ProductAttribute\\\\><AttributeName\\\\>Test2</AttributeName\\\\><AttributeValue xsi:type=\\\\\"xsd:dateTime\\\\\"\\\\>2007-03-09T10:42:00.6517547-10:00</AttributeValue\\\\><CurrencyOffset\\\\>-100</CurrencyOffset\\\\></ProductAttribute\\\\></ArrayOfProductAttribute\\\\>\", \"CategoryID\": \"1\", \"CreatedOn\": \"2007-05-22T19:43:04-06:00\", \"DateCreated\": \"2007-05-22T19:44:42-06:00\", \"Deleted\": \"false\", \"Discontinued\": \"false\", \"ModifiedBy\": \"Unit Test\", \"ModifiedOn\": \"2007-05-24T06:31:51-06:00\", \"ProductGUID\": \"52c7760f-6b66-46e7-a99b-67cf142f2a6c\", \"ProductID\": \"1\", \"ProductName\": \"Chai\", \"QuantityPerUnit\": \"10 boxes x 20 bags\", \"ReorderLevel\": \"10\", \"SupplierID\": \"16\", \"UnitPrice\": \"50.0000\", \"UnitsInStock\": \"39\", \"UnitsOnOrder\": \"0\" }, {\"AttributeXML\": null, \"CategoryID\": \"1\", \"CreatedOn\": \"2007-05-22T19:43:04-06:00\", \"DateCreated\": \"2007-05-22T19:44:42-06:00\", \"Deleted\": \"false\", \"Discontinued\": \"false\", \"ModifiedBy\": \"Unit Test\", \"ModifiedOn\": \"2007-05-24T06:31:51-06:00\", \"ProductGUID\": \"729360ee-ec54-4e02-9973-2fb3fd2cffac\", \"ProductID\": \"2\", \"ProductName\": \"Chang\", \"QuantityPerUnit\": \"24 - 12 oz bottles\", \"ReorderLevel\": \"25\", \"SupplierID\": \"1\", \"UnitPrice\": \"50.0000\", \"UnitsInStock\": \"17\", \"UnitsOnOrder\": \"40\" } ] }}";
            Assert.AreEqual(expectedResult, content.Content);
        }

    }
}
