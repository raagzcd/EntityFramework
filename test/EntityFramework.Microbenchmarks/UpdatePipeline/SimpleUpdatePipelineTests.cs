﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.Models.Orders;
using Microsoft.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace EntityFramework.Microbenchmarks.UpdatePipeline
{
    public class SimpleUpdatePipelineTests
    {
        private static string _connectionString = String.Format(@"Server={0};Database=Perf_UpdatePipeline_Simple;Integrated Security=True;MultipleActiveResultSets=true;", TestConfig.Instance.DataSource);

        [Fact]
        public void Insert()
        {
            new TestDefinition
            {
                TestName = "UpdatePipeline_Simple_Insert",
                IterationCount = 100,
                WarmupCount = 5,
                RunWithCollector = Insert,
                Setup = EnsureDatabaseSetup
            }.RunTest();
        }

        private static void Insert(MetricCollector collector)
        {
            using (var context = new OrdersContext(_connectionString))
            {
                using (context.Database.AsRelational().Connection.BeginTransaction())
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        context.Customers.Add(new Customer { Name = "New Customer " + i });
                    }

                    collector.Start();
                    var records = context.SaveChanges();
                    collector.Stop();

                    Assert.Equal(1000, records);
                }
            }
        }

        [Fact]
        public void Update()
        {
            new TestDefinition
            {
                TestName = "UpdatePipeline_Simple_Update",
                IterationCount = 100,
                WarmupCount = 5,
                RunWithCollector = Update,
                Setup = EnsureDatabaseSetup
            }.RunTest();
        }

        private static void Update(MetricCollector collector)
        {
            using (var context = new OrdersContext(_connectionString))
            {
                using (context.Database.AsRelational().Connection.BeginTransaction())
                {
                    foreach (var customer in context.Customers)
                    {
                        customer.Name += " Modified";
                    }

                    collector.Start();
                    var records = context.SaveChanges();
                    collector.Stop();

                    Assert.Equal(1000, records);
                }
            }
        }

        [Fact]
        public void Delete()
        {
            new TestDefinition
            {
                TestName = "UpdatePipeline_Simple_Delete",
                IterationCount = 100,
                WarmupCount = 5,
                RunWithCollector = Delete,
                Setup = EnsureDatabaseSetup
            }.RunTest();
        }

        private static void Delete(MetricCollector collector)
        {
            using (var context = new OrdersContext(_connectionString))
            {
                using (context.Database.AsRelational().Connection.BeginTransaction())
                {
                    foreach (var customer in context.Customers)
                    {
                        context.Customers.Remove(customer);
                    }

                    collector.Start();
                    var records = context.SaveChanges();
                    collector.Stop();

                    Assert.Equal(1000, records);
                }
            }
        }

        [Fact]
        public void Mixed()
        {
            new TestDefinition
            {
                TestName = "UpdatePipeline_Simple_Mixed",
                IterationCount = 100,
                WarmupCount = 5,
                RunWithCollector = Mixed,
                Setup = EnsureDatabaseSetup
            }.RunTest();
        }

        private static void Mixed(MetricCollector collector)
        {
            using (var context = new OrdersContext(_connectionString))
            {
                using (context.Database.AsRelational().Connection.BeginTransaction())
                {
                    var customers = context.Customers.ToArray();

                    for (int i = 0; i < 333; i++)
                    {
                        context.Customers.Add(new Customer { Name = "New Customer " + i });
                    }

                    for (int i = 0; i < 1000; i += 3)
                    {
                        context.Customers.Remove(customers[i]);
                    }

                    for (int i = 1; i < 1000; i += 3)
                    {
                        customers[i].Name += " Modified";
                    }

                    collector.Start();
                    var records = context.SaveChanges();
                    collector.Stop();

                    Assert.Equal(1000, records);
                }
            }
        }

        private static void EnsureDatabaseSetup()
        {
            OrdersSeedData.EnsureCreated(
                _connectionString,
                productCount: 0,
                customerCount: 1000,
                ordersPerCustomer: 0,
                linesPerOrder: 0);
        }
    }
}
