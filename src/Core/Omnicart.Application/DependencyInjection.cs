using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace OmniCart.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // ১. MediatR রেজিস্টার করা (CQRS Commands ও Queries হ্যান্ডেল করার জন্য)

            services.AddMediatR(configuration => {
                configuration.RegisterServicesFromAssembly(assembly);
            });

            // ২. FluentValidation রেজিস্টার করা (অটোমেটিক ইনপুট ভ্যালিডেশনের জন্য)

            services.AddValidatorsFromAssembly(assembly);

            return services;
        }   
    }
}
