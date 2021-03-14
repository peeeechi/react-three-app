import React from 'react';

export type ListTableProps = {
    obj: {[key: string]: string|number|null|undefined}
}

export default function ListTable({obj}: ListTableProps) {

    const tableItems = Object.entries(obj).map((item, index) => {
        const classN = (index % 2 ==0)? "border px-2 py-1":"border px-2 py-1"
        return (
            <tr>
                {}
                <th className={classN}>{item[0]}</th><td className={classN}>{(item[1] != null)? item[1] : "-"}</td>
            </tr>
        );
    });
    
    return (
        <table className="border border-collapse">
            {tableItems}
        </table>
    )
}