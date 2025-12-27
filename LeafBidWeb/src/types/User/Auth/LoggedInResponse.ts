import {User} from "@/types/User/User";

export interface LoggedInResponse {
    loggedIn: boolean;
    userData: User
}