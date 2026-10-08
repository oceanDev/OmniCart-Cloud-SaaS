import React from 'react';

// Cards

interface CardProps extends React.HTMLAttributes<HTMLDivElement>{

    children: React.ReactNode;

}

export const Card = ({children, className = '' ,...props} : CardProps ) => {

    return (
        <div className={`rounded-2xl bg-slate-900 border border-slate-800 shadow-xl overflow-hidden transition-all duration-200 hover:border-slate-700 ${className}`}  {...props}>
             {children}
        </div>
    );

};

// Card Header

interface CardHeaderProps extends React.HTMLAttributes<HTMLDivElement> {

  children: React.ReactNode;

}

const CardHeader = ({ children, className = '', ...props }: CardHeaderProps) => {

  return (
    <div className={`p-6 border-b border-slate-800/80 space-y-1.5 ${className}`} {...props}>
      {children}
    </div>
  );

};

// Card Title

interface CardTitleProps extends React.HTMLAttributes<HTMLDivElement>{

    children: React.ReactNode;

}

export const CardTitle = ({children, className = '' ,...props} : CardTitleProps ) => {

    return (
        <h3 className={`text-xl font-bold text-white tracking-tight ${className}`} {...props}> 
            {children}
        </h3>
    );

};


// Card Description

interface CardDescriptionProps extends React.HTMLAttributes<HTMLParagraphElement> {

  children: React.ReactNode;

}
const CardDescription = ({ children, className = '', ...props }: CardDescriptionProps) => {

  return (
    <p className={`text-sm text-slate-400 ${className}`} {...props}>
      {children}
    </p>
  );

};

// Sub-Component: Content

interface CardContentProps extends React.HTMLAttributes<HTMLDivElement> {

  children: React.ReactNode;

}
const CardContent = ({ children, className = '', ...props }: CardContentProps) => {

  return (
    <div className={`p-6 ${className}`} {...props}>
      {children}
    </div>
  );

};

 // Card Footer

interface CardFooterProps extends React.HTMLAttributes<HTMLDivElement> {
  children: React.ReactNode;
}
const CardFooter = ({ children, className = '', ...props }: CardFooterProps) => {

  return (
    <div className={`p-6 bg-slate-950/40 border-t border-slate-800/80 flex items-center justify-between ${className}`} {...props}>
      {children}
    </div>
  );

};


// The Compound Component Binding

Card.Header = CardHeader;
Card.Title = CardTitle;
Card.Description = CardDescription;
Card.Content = CardContent;
Card.Footer = CardFooter;