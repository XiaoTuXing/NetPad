import {bindable} from "aurelia";
import {DataConnection} from "@domain";

export class DataConnectionName {
    @bindable public connection: DataConnection;

    public get iconUrl(): string {
        switch (this.connection.type) {
            case "MSSQLServer":
                return "/img/mssql.png";
            case "PostgreSQL":
                return "/img/postgresql2.png";
            default:
                return "";
        }
    }
}
