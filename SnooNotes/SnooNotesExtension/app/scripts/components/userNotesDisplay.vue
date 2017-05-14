<template>
    <transition name="fade">
        <div class="SNNotesDisplay" v-if="showNotes" :style="displayStyle" v-draggable="'.SNHeader'">
            <div class="SNHeader">
                <a class="SNCloseNote SNClose" @click="close">[x]</a>
            </div>
            <table v-if="!notes.noNotes && !notes.loading">
                <tbody is="transition-group" name="fade">
                    <tr v-for="note in notes" :style="noteTypeStyle(note.SubName, note.NoteTypeID)" transition="fade" :key="note.NoteID">
                        <td class="SNSubName">
                            <a :href="'https://reddit.com/r/'+note.SubName">{{note.SubName}}</a>
                            <span v-if="note.ParentSubreddit">
                                <br />via
                                <br />
                                <a :href="'https://reddit.com/r/'+note.ParentSubreddit">{{note.ParentSubreddit}}</a>
                            </span>
                        </td>
                        <td class="SNSubmitter">
                            <span>{{note.Submitter}}</span>
                            <br />
                            <a :href="note.Url" style="white-space:pre;">{{new Date(note.Timestamp).toLocaleString().replace(', ', '\n')}}</a>
                        </td>
                        <td class="SNMessage">
                            <p>{{note.Message}}</p>
                            <a class="SNDeleteNote" @click="deleteNote(note.NoteID)" v-if="!note.ParentSubreddit || modSubs.findIndex(s=>s.SubName == note.ParentSubreddit) > -1">[x]</a>
                        </td>
                    </tr>
                </tbody>
            </table>
            <div class="SNNewNoteContainer">
                <div class="SNNewNote">
                    <select class="SNNewNoteSub" v-model="newNote.newNoteSubIndex" :class="{SNError:$v.newNote.newNoteSubIndex.$error }">
                        <option value="-1" disabled>--Select a Sub--</option>
                        <option :value="modSub.index" v-if="isModdedSub">{{modSub.name}}</option>
                        <option value="-2" v-if="isModdedSub" disabled>---------</option>
                        <option v-for="sub in otherSubs" v-if="otherSubs.length >0" :value="sub.index">{{sub.name}}</option>
                    </select>
                    <textarea placeholder="Add a new note for user..." class="SNNewMessage" v-model="newNote.message" />
                    <button type="button" class="SNBtnSubmit SNNewNoteSubmit" @click="submit" :disabled="submitting">Submit</button>
                </div>
                <div class="SNNoteType" :class="{SNError:$v.newNote.newNoteTypeID.$error }">
                    <label class="SNTypeRadio" v-for="nt in noteTypes" :style="noteTypeStyle(newNote.newNoteSubIndex,nt.NoteTypeID)">
                        <input type="radio" name="SNType" :value="nt.NoteTypeID" v-model="newNote.newNoteTypeID">{{nt.DisplayName}}</label>
                </div>
                <div class="SNNewError">
                    <p v-if="$v.newNote.newNoteTypeID.$error">Shucks! You forgot the note type...</p>
                    <p v-if="$v.newNote.newNoteSubIndex.$error">Select a subby you fool!</p>
                </div>
            </div>
        </div>
    </transition>
</template>
<script>
import { store } from '../redux/contentScriptStore';
import { draggable } from './directives/draggable';
import { validationMixin } from 'vuelidate'
import { required, between } from 'vuelidate/lib/validators'
import axios from 'axios';
import {showNotesHub} from '../showNotesHub';

export default {
    name: 'user-notes',
    //props: ['username', 'subreddit', 'url', 'showNotes'],
    directives: { 'draggable': draggable },
    mixins: [validationMixin],
    data() {
        return {
            snInfo: this.$select('snoonotes_info as snInfo'),
            username: "",
            subreddit: "",
            url: "",
            showNotes: false,
            allNotes: this.$select('notes as allNotes'),
            displayStyle: {
                display: 'none',
                position: 'absolute',
                top: '0px',
                right: '0px',

            },
            newNote: {
                message: "",
                newNoteTypeID: null,
                newNoteSubIndex: -1,
            },
            submitting: false
        }
    },
    validations: {
        newNote: {
            newNoteSubIndex: {
                required,
                'notNegative': function (value) {
                    return value > -1;
                }
            },
            newNoteTypeID: {
                required
            }
        }
    },
    computed: {
        
        userNotes: function(){
            return this.allNotes[this.username];
        },
        hasNotes: function () {
            return this.usersWithNotes.indexOf(this.username) > -1;
        },
        usersWithNotes: function () {
            return this.snInfo.users_with_notes;
        },
        modSubs: function () {
            return this.snInfo.modded_subs.map((s, i) => { return { name: s.SubName, id: s.SubredditID, index: i } });
        },
        modSub: function(){
            return this.modSubs.filter(sub => sub.name == this.subreddit)[0];
        },
        otherSubs: function () {
            return this.modSubs.filter(sub => sub.name != this.subreddit);
        },
        notes: function () {
            return this.userNotes || { noNotes: true }
        },
        noteTypes: function () {
            if (this.newNote.newNoteSubIndex == -1) return {}
            return this.snInfo.modded_subs[this.newNote.newNoteSubIndex].Settings.NoteTypes;
        },
        isModdedSub: function () {
            if (this.subreddit && this.modSubs.findIndex(sub => sub.name == this.subreddit) > -1) {
                return true;
            }
            return false;
        }
    },
    methods: {
        close: function () {
            this.showNotes = false;
            this.newNote.message = "";
            this.newNoteTypeID = -1;
            document.removeEventListener('click', this.clickWatch, true);
        },
        noteTypeStyle: function (sub, ntID) {
            let subIndex = isNaN(sub) ? this.snInfo.modded_subs.findIndex(subreddit => subreddit.SubName == sub) : sub;
            let nt = this.snInfo.modded_subs[subIndex].Settings.NoteTypes.filter(nt => nt.NoteTypeID == ntID)[0]
            let style = {
                color: '#' + nt.ColorCode
            }
            if (nt.Bold) style.fontWeight = "bold";
            if (nt.Italic) style.fontStyle = "italic";
            return style;
        },
        show: function (e) {
            //this.displayStyle.top = e.target.offsetTop + 15 + 'px';
            //this.displayStyle.left = e.target.offsetLeft + 20 + 'px';
            this.displayStyle.top = e.pageY + 5 + 'px';
            this.displayStyle.left = e.pageX + 15 + 'px'

            this.displayStyle.display = 'block';
            this.showNotes = true;
            if (this.subreddit) {
                this.newNote.newNoteSubIndex = this.modSubs.findIndex(sub => sub.name == this.subreddit)
            } else {
                this.newNote.newNoteSubIndex = -1
            }
            document.addEventListener('click', this.clickWatch, true);
        },
        clickWatch: function (e) {
            if (!this.$el.contains(e.target)) this.close();
        },
        submit: function () {
            this.$v.newNote.$touch();
            if (this.$v.newNote.$invalid) { return; }
            this.submitting = true;
            axios.post('Note', { NoteTypeID: this.newNote.newNoteTypeID, SubName: this.modSubs[this.newNote.newNoteSubIndex].name, Message: this.newNote.message, AppliesToUsername: this.username, Url: this.url })
                .then(() => {
                    this.submitting = false;
                    this.newNote.message = "";
                }, (e) => {
                    this.submitting = false;
                    this.$toasted.error("Failed to submit new note.");
                })
        },
        deleteNote: function (id) {
            axios.delete('Note?id=' + id);
        }
    },
    watch: {
        'newNote.newNoteSubIndex': function (newIndex) {
            this.newNote.newNoteTypeID = null;
            this.$v.newNote.$reset();
        }
    },
    mounted:function(){
        showNotesHub.$on('showNotes',(e)=>{
            this.username = e.username;
            this.url = e.url;
            this.subreddit = e.subreddit;
            this.showNotes = true;
            this.show(e.event)
        });
    },
    beforeDestroy: function () {
        document.removeEventListener('click', this.clickWatch, true);
    }
}
</script>
<style lang="scss">
@import "~styles/_vars.scss";

.SNNotesDisplay {
    .SNHeader {
        margin-left: -10px;
        margin-right: -10px;
        margin-bottom: 10px;
        .SNClose {
            display: inline-block;
            cursor: pointer;
            color: white;
            background-color: $accent;
            font-weight: bold;
            padding: 2px 10px;
            margin-top: -1px;
            &:hover {
                background-image: linear-gradient(to bottom, darken($accent, 10%), darken($accent, 25%));
            }
        }
    }
    table {
        border-collapse: separate;
        border-spacing: 0;
    }
    td {
        border: solid 1px $gray;
        border-style: none solid solid none;
        padding: 10px;
    }
    tr:first-child td:first-child {
        border-top-left-radius: 10px;
    }
    tr:first-child td:last-child {
        border-top-right-radius: 10px;
    }
    tr:last-child td:first-child {
        border-bottom-left-radius: 10px;
    }
    tr:last-child td:last-child {
        border-bottom-right-radius: 10px;
    }
    tr:first-child td {
        border-top-style: solid;
    }
    tr td:first-child {
        border-left-style: solid;
    }
}

.SNDeleteNote {
    position: absolute;
    top: 1px;
    right: 5px;
    cursor: pointer;
}
</style>

