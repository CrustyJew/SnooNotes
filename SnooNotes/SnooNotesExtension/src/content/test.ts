import { Vue, Component } from 'av-ts'

@Component({
    template: `
    <h1>{{message }}</h1>
    `})
export class TestComponent extends Vue {
    message: string = "yoyoyo";
}