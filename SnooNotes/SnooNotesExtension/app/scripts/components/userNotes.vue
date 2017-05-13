<template>
    <span v-if="snInfo.modded_subs.length > 0" class="SNUserNotes" @click.stop>
    
        <span @click="show">[SN]</span>
    </span>
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
    props: ['username', 'subreddit', 'url'],
    directives: { 'draggable': draggable },
    mixins: [validationMixin],
    data() {
        return {
            snInfo: this.$select('snoonotes_info as snInfo'),
            userNotes: this.$select('notes.' + this.username + ' as userNotes'),
            showNotes: false,
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
        hasNotes: function () {
            return this.usersWithNotes.indexOf(this.username) > -1;
        },
        usersWithNotes: function () {
            return this.snInfo.users_with_notes;
        },
        modSubs: function () {
            return this.snInfo.modded_subs.map((s, i) => { return { name: s.SubName, id: s.SubredditID, index: i } });
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
            
            showNotesHub.$emit('showNotes',{event:e, subreddit:this.subreddit, username:this.username, url:this.url});
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

.SNUserNotes {
    cursor: pointer;
}
</style>