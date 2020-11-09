import React, {Children, useState} from 'react';

interface SideBarProps {
    children: React.ReactNode;
    header?: string;
}

export default function SideBar({children, header="Sidebar header"}: SideBarProps) {
    

    return (
        <>
            <input type="checkbox" id="check"/>
            <label htmlFor="check">
                <i className="fas fa-bars" id="btn"></i>
                <i className="fas fa-times" id="cancel"></i>
            </label>
            <div className="sidebar">
            <header>{header}</header>
                {children}
            </div>
        </>
    );
}